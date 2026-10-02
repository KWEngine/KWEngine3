float fogDensityAtHeight(float y, float baseDensity, float baseHeight, float falloff)
{
    return baseDensity * exp(clamp(-falloff * (y - baseHeight), -80.0, 80.0));
}

float fogOpticalDepthHeight(vec3 rayOrigin, vec3 rayDir, float dist, float baseDensity, float baseHeight, float falloff)
{
    // density(t) = density(0) * exp(-k * t)
    float k = falloff * rayDir.y;
    if (abs(k * dist) > 0.001)
    {
        // (density(0) - density(dist)) / k
        float d0 = fogDensityAtHeight(rayOrigin.y, baseDensity, baseHeight, falloff);
        float d1 = fogDensityAtHeight(rayOrigin.y + rayDir.y * dist, baseDensity, baseHeight, falloff);
        return (d0 - d1) / k;
    }
    return fogDensityAtHeight(rayOrigin.y + rayDir.y * dist * 0.5, baseDensity, baseHeight, falloff) * dist;
}

float fogAmountFromOpticalDepth(float opticalDepth)
{
    return 1.0 - exp(-opticalDepth);
}

layout(std140) uniform uBlockFog
{
    vec4 uFogColorDensity;   // xyz = color, w = thickness (0 = no fog)
    vec4 uFogHeightFalloff;  // x = base height, y = falloff to the top (0 = same from bottom to top), zw = not used (currently)
    vec4 uFogNoiseParams;    // x = strength of patches (0..1), y = noise frequency (1 / world size of one noise tile), z = amplitude of the wavy top (world units), w = quality (0 = wavy top only, 1 = wavy top + patches)
    vec4 uFogNoiseOffset;    // xyz = wind offset in noise texture space (wrapped to 0..1), w = slow evolution offset (wrapped to 0..1)
    vec4 uFogNoiseLod;       // x = noise texels covered by one pixel per world unit of distance (mip selection), y = noise features per world unit of ray path (averaging along long rays), zw = not used (currently)
};

// tileable 3D noise (values 0..1, mean 0.5) on texture unit 15, bound once per frame by RendererFog
uniform sampler3D uTextureFogNoise;

// mip level for a noise lookup at a given distance from the camera (avoids shimmering in the distance)
float fogNoiseLod(float distanceToCamera)
{
    return log2(max(distanceToCamera * uFogNoiseLod.x, 1.0));
}

// additional mip level when one lookup has to stand for a whole stretch of the view ray:
// the longer the stretch, the more the noise averages out (prevents streaks in the far background)
float fogPathLod(float pathLength)
{
    return log2(max(pathLength * uFogNoiseLod.y, 1.0));
}

// noise value (0..1, mean 0.5) at a world position, drifting with the wind
float fogNoiseAt(vec3 worldPosition, float lod)
{
    vec3 uvw = worldPosition * uFogNoiseParams.y + uFogNoiseOffset.xyz + vec3(0.0, uFogNoiseOffset.w, 0.0);
    return textureLod(uTextureFogNoise, uvw, lod).r;
}

// base height of the height fog at a position on the ground plane (wavy top)
float fogBaseHeightAt(vec2 worldXZ, float lod)
{
    vec3 uvw = vec3(worldXZ.x * uFogNoiseParams.y + uFogNoiseOffset.x, uFogNoiseOffset.w, worldXZ.y * uFogNoiseParams.y + uFogNoiseOffset.z);
    float n = textureLod(uTextureFogNoise, uvw, lod).r;
    return uFogHeightFalloff.x + (n * 2.0 - 1.0) * uFogNoiseParams.z;
}

// fog density at a single point - the reference model for ray marching (volumetric light);
// fogAmount() below is the analytic approximation of its integral along the view ray
float fogDensity(vec3 worldPosition, float distanceToCamera)
{
    float lod = fogNoiseLod(distanceToCamera);
    float baseHeight = uFogHeightFalloff.x;
    if (uFogNoiseParams.z > 0.0 && uFogHeightFalloff.y > 0.0)
        baseHeight = fogBaseHeightAt(worldPosition.xz, lod);
    float d = fogDensityAtHeight(worldPosition.y, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y);
    if (uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5)
        d *= 1.0 + uFogNoiseParams.x * (2.0 * fogNoiseAt(worldPosition, lod) - 1.0);
    return d;
}

// return value: 0 = clear sight, 1 = completely fogged
float fogAmount(vec3 cameraPosition, vec3 worldPosition)
{
    if (uFogColorDensity.w <= 0.0)
        return 0.0;
    vec3 cameraToFragment = worldPosition - cameraPosition;
    float dist = length(cameraToFragment);
    vec3 rayDir = cameraToFragment / max(dist, 0.0001);

    // (A) wavy top: local base height where the ray passes the fog top
    //     (camera inside the fog: at the hit point; camera above: where the ray dives in; smooth blend in between)
    float baseHeight = uFogHeightFalloff.x;
    if (uFogNoiseParams.z > 0.0 && uFogHeightFalloff.y > 0.0)
    {
        float above = clamp((cameraPosition.y - uFogHeightFalloff.x) / uFogNoiseParams.z, 0.0, 1.0);
        bool crosses = above > 0.0 && rayDir.y < -0.0001;
        float hEnd = uFogHeightFalloff.x;
        float hCross = uFogHeightFalloff.x;
        if (!crosses || above < 1.0)
            hEnd = fogBaseHeightAt(worldPosition.xz, max(fogNoiseLod(dist), fogPathLod(dist)));
        if (crosses)
        {
            float tCross = min((uFogHeightFalloff.x - cameraPosition.y) / rayDir.y, dist);
            hCross = fogBaseHeightAt((cameraPosition + rayDir * tCross).xz, max(fogNoiseLod(tCross), fogPathLod(dist - tCross)));
        }
        baseHeight = crosses ? mix(hEnd, hCross, above) : hEnd;
    }
    float opticalDepth = fogOpticalDepthHeight(cameraPosition, rayDir, dist, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y);

    // (B) patches: average noise factor at 3 points along the ray (coarser mip levels in the distance -> factor ~1)
    if (uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5)
    {
        float n = 0.0;
        float segmentLod = fogPathLod(dist / 3.0);
        for (int i = 0; i < 3; i++)
        {
            float t = dist * (float(i) + 0.5) / 3.0;
            n += fogNoiseAt(cameraPosition + rayDir * t, max(fogNoiseLod(t), segmentLod));
        }
        opticalDepth *= 1.0 + uFogNoiseParams.x * (2.0 * (n / 3.0) - 1.0);
    }
    return fogAmountFromOpticalDepth(opticalDepth);
}

// mixes fog color with world color
vec3 applyFog(vec3 color, vec3 worldPosition, vec3 cameraPosition)
{
    return mix(color, uFogColorDensity.xyz, fogAmount(cameraPosition, worldPosition));
}
