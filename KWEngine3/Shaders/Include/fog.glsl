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

#define FOG_MAX_VOLUMES 16

struct FogVolume
{
    vec4 worldToLocal0;   // rows of the inverse model matrix (world -> unit box -0.5..0.5)
    vec4 worldToLocal1;
    vec4 worldToLocal2;
    vec4 colorDensity;    // xyz = color, w = density
    vec4 heightParams;    // x = base height (lowest point), y = falloff
    vec4 innerExtent;     // xyz = half extent of the dense core (soft edge)
};

layout(std140) uniform uBlockFog
{
    vec4 uFogColorDensity;   // xyz = color, w = thickness (0 = no fog)
    vec4 uFogHeightFalloff;  // x = base height, y = falloff to the top (0 = same from bottom to top), zw = not used (currently)
    vec4 uFogNoiseParams;    // x = strength of patches (0..1), y = noise frequency (1 / world size of one noise tile), z = amplitude of the wavy top (world units), w = quality (0 = wavy top only, 1 = wavy top + patches)
    vec4 uFogNoiseOffset;    // xyz = wind offset in noise texture space (wrapped to 0..1), w = slow evolution offset (wrapped to 0..1)
    vec4 uFogNoiseLod;       // x = noise texels covered by one pixel per world unit of distance (mip selection), y = noise features per world unit of ray path (averaging along long rays), zw = not used (currently)
    vec4 uFogVolumeInfo;     // x = number of active volumes
    FogVolume uFogVolumes[FOG_MAX_VOLUMES];
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

vec3 fogVolumeToLocal(int i, vec3 p)
{
    vec4 p4 = vec4(p, 1.0);
    return vec3(dot(uFogVolumes[i].worldToLocal0, p4), dot(uFogVolumes[i].worldToLocal1, p4), dot(uFogVolumes[i].worldToLocal2, p4));
}

vec3 fogVolumeDirToLocal(int i, vec3 d)
{
    return vec3(dot(uFogVolumes[i].worldToLocal0.xyz, d), dot(uFogVolumes[i].worldToLocal1.xyz, d), dot(uFogVolumes[i].worldToLocal2.xyz, d));
}

// entry and exit distance of the ray inside a local box (no hit if x >= y)
vec2 fogRayBox(vec3 localOrigin, vec3 localDir, vec3 halfExtent, float maxT)
{
    vec3 d = vec3(abs(localDir.x) < 1e-8 ? 1e-8 : localDir.x, abs(localDir.y) < 1e-8 ? 1e-8 : localDir.y, abs(localDir.z) < 1e-8 ? 1e-8 : localDir.z);
    vec3 t0 = (-halfExtent - localOrigin) / d;
    vec3 t1 = (halfExtent - localOrigin) / d;
    vec3 tMin = min(t0, t1);
    vec3 tMax = max(t0, t1);
    return vec2(max(max(max(tMin.x, tMin.y), tMin.z), 0.0), min(min(min(tMax.x, tMax.y), tMax.z), maxT));
}

// fog at a single point (reference for ray marching): xyz = density weighted color, w = density
vec4 fogDensity(vec3 worldPosition, float distanceToCamera)
{
    float lod = fogNoiseLod(distanceToCamera);
    bool patches = uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5;
    float n = (patches || uFogNoiseParams.z > 0.0) ? fogNoiseAt(worldPosition, lod) : 0.5;
    float factor = patches ? 1.0 + uFogNoiseParams.x * (2.0 * n - 1.0) : 1.0;
    vec4 result = vec4(0.0);
    if (uFogColorDensity.w > 0.0)
    {
        float baseHeight = uFogHeightFalloff.x;
        if (uFogNoiseParams.z > 0.0 && uFogHeightFalloff.y > 0.0)
            baseHeight = fogBaseHeightAt(worldPosition.xz, lod);
        float d = fogDensityAtHeight(worldPosition.y, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y) * factor;
        result += vec4(uFogColorDensity.xyz * d, d);
    }
    int volumeCount = int(uFogVolumeInfo.x + 0.5);
    for (int i = 0; i < volumeCount; i++)
    {
        vec3 l = abs(fogVolumeToLocal(i, worldPosition));
        if (any(greaterThan(l, vec3(0.5))))
            continue;
        vec3 ramp = clamp((vec3(0.5) - l) / max(vec3(0.5) - uFogVolumes[i].innerExtent.xyz, vec3(0.0001)), 0.0, 1.0);
        float baseHeight = uFogVolumes[i].heightParams.x;
        if (uFogNoiseParams.z > 0.0 && uFogVolumes[i].heightParams.y > 0.0)
            baseHeight += (n * 2.0 - 1.0) * uFogNoiseParams.z;
        float d = fogDensityAtHeight(worldPosition.y, uFogVolumes[i].colorDensity.w, baseHeight, uFogVolumes[i].heightParams.y) * min(min(ramp.x, ramp.y), ramp.z) * factor;
        result += vec4(uFogVolumes[i].colorDensity.xyz * d, d);
    }
    return result;
}

// global fog along one ray: base height of the wavy top and the averaged patch factor
void fogGlobalSetup(vec3 cameraPosition, vec3 rayDir, float dist, vec3 worldPosition, out float baseHeight, out float noiseFactor)
{
    baseHeight = uFogHeightFalloff.x;
    noiseFactor = 1.0;
    if (uFogColorDensity.w <= 0.0)
        return;

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

    if (uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5)
    {
        float n = 0.0;
        float segmentLod = fogPathLod(dist / 3.0);
        for (int i = 0; i < 3; i++)
        {
            float t = dist * (float(i) + 0.5) / 3.0;
            n += fogNoiseAt(cameraPosition + rayDir * t, max(fogNoiseLod(t), segmentLod));
        }
        noiseFactor = 1.0 + uFogNoiseParams.x * (2.0 * (n / 3.0) - 1.0);
    }
}

// mean soft edge factor between t0 and t1, weighted by the height falloff (heightSlope = falloff * rayDir.y)
float fogVolumeRamp(vec3 localOrigin, vec3 localDir, vec3 invWidth, float t0, float t1, float heightSlope)
{
    float r = 0.0;
    float weightSum = 0.0;
    for (int k = 0; k < 4; k++)
    {
        float t = mix(t0, t1, (float(k) + 0.5) * 0.25);
        vec3 ramp = clamp((vec3(0.5) - abs(localOrigin + localDir * t)) * invWidth, 0.0, 1.0);
        float weight = exp(clamp(-heightSlope * (t - t0), -80.0, 80.0));
        r += min(min(ramp.x, ramp.y), ramp.z) * weight;
        weightSum += weight;
    }
    return r / weightSum;
}

void fogRampPeakPair(float r, float k, vec3 f, vec3 kf, inout float best, inout float tBest)
{
    vec3 t = (f - r) / (k + kf);
    vec3 p = r + k * t;
    if (p.x < best) { best = p.x; tBest = t.x; }
    if (p.y < best) { best = p.y; tBest = t.y; }
    if (p.z < best) { best = p.z; tBest = t.z; }
}

// ray distance where the soft edge ramp peaks: lowest crossing of a rising and a falling ramp line
float fogVolumeRampPeak(vec3 localOrigin, vec3 localDir, vec3 invWidth)
{
    vec3 u = mix(-localOrigin, localOrigin, step(0.0, localDir));
    vec3 k = max(abs(localDir), vec3(1e-6)) * invWidth;
    vec3 rising = (0.5 + u) * invWidth;
    vec3 falling = (0.5 - u) * invWidth;
    float best = 1e30;
    float tBest = 0.0;
    fogRampPeakPair(rising.x, k.x, falling, k, best, tBest);
    fogRampPeakPair(rising.y, k.y, falling, k, best, tBest);
    fogRampPeakPair(rising.z, k.z, falling, k, best, tBest);
    return tBest;
}

float fogSegmentDepth(vec3 cameraPosition, vec3 rayDir, float t0, float t1, float density, float baseHeight, float falloff)
{
    return t1 > t0 ? fogOpticalDepthHeight(cameraPosition + rayDir * t0, rayDir, t1 - t0, density, baseHeight, falloff) : 0.0;
}

// optical depth of volume i along the ray: x = depth (0 = missed), yz = ray segment inside the box
vec3 fogVolumeDepth(int i, vec3 cameraPosition, vec3 rayDir, float dist)
{
    vec3 localOrigin = fogVolumeToLocal(i, cameraPosition);
    vec3 localDir = fogVolumeDirToLocal(i, rayDir);
    vec2 outer = fogRayBox(localOrigin, localDir, vec3(0.5), dist);
    if (outer.y <= outer.x)
        return vec3(0.0);
    float tMid = 0.5 * (outer.x + outer.y);

    float density = uFogVolumes[i].colorDensity.w;
    float baseHeight = uFogVolumes[i].heightParams.x;
    float falloff = uFogVolumes[i].heightParams.y;
    bool wave = uFogNoiseParams.z > 0.0 && falloff > 0.0;
    bool patches = uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5;
    float factor = 1.0;
    if (wave || patches)
    {
        float n = fogNoiseAt(cameraPosition + rayDir * tMid, max(fogNoiseLod(tMid), fogPathLod(outer.y - outer.x)));
        if (wave)
            baseHeight += (n * 2.0 - 1.0) * uFogNoiseParams.z;
        if (patches)
            factor = 1.0 + uFogNoiseParams.x * (2.0 * n - 1.0);
    }

    float tau;
    vec3 shell = vec3(0.5) - uFogVolumes[i].innerExtent.xyz;
    if (max(max(shell.x, shell.y), shell.z) <= 0.0)
    {
        tau = fogSegmentDepth(cameraPosition, rayDir, outer.x, outer.y, density, baseHeight, falloff);
    }
    else
    {
        // soft edge: dense core exact, rising and falling part of the ramp sampled separately
        vec3 invWidth = 1.0 / max(shell, vec3(0.0001));
        float heightSlope = falloff * rayDir.y;
        vec2 inner = fogRayBox(localOrigin, localDir, uFogVolumes[i].innerExtent.xyz, dist);
        tau = 0.0;
        if (inner.y > inner.x)
            tau = fogSegmentDepth(cameraPosition, rayDir, inner.x, inner.y, density, baseHeight, falloff);
        else
            inner = vec2(clamp(fogVolumeRampPeak(localOrigin, localDir, invWidth), outer.x, outer.y));
        tau += fogSegmentDepth(cameraPosition, rayDir, outer.x, inner.x, density, baseHeight, falloff) * fogVolumeRamp(localOrigin, localDir, invWidth, outer.x, inner.x, heightSlope)
             + fogSegmentDepth(cameraPosition, rayDir, inner.y, outer.y, density, baseHeight, falloff) * fogVolumeRamp(localOrigin, localDir, invWidth, inner.y, outer.y, heightSlope);
    }
    return vec3(tau * factor, outer);
}

// fog between camera and world position: xyz = in-scattered fog color, w = remaining transmittance
vec4 fogIntegrate(vec3 cameraPosition, vec3 worldPosition)
{
    int volumeCount = int(uFogVolumeInfo.x + 0.5);
    if (uFogColorDensity.w <= 0.0 && volumeCount == 0)
        return vec4(0.0, 0.0, 0.0, 1.0);

    vec3 cameraToFragment = worldPosition - cameraPosition;
    float dist = length(cameraToFragment);
    vec3 rayDir = cameraToFragment / max(dist, 0.0001);

    float baseHeight;
    float noiseFactor;
    fogGlobalSetup(cameraPosition, rayDir, dist, worldPosition, baseHeight, noiseFactor);

    // volumes hit by the ray (optical depth spread evenly over the segment)
    vec2 hitSegment[FOG_MAX_VOLUMES];
    float hitExtinction[FOG_MAX_VOLUMES];
    int hitIndex[FOG_MAX_VOLUMES];
    int hits = 0;
    for (int i = 0; i < volumeCount; i++)
    {
        vec3 v = fogVolumeDepth(i, cameraPosition, rayDir, dist);
        if (v.x <= 0.0)
            continue;
        hitSegment[hits] = v.yz;
        hitExtinction[hits] = v.x / max(v.z - v.y, 0.000001);
        hitIndex[hits] = i;
        hits++;
    }

    // walk from segment border to segment border, overlapping media mixed per interval
    vec3 inscatter = vec3(0.0);
    float transmittance = 1.0;
    float t = 0.0;
    for (int s = 0; s <= 2 * FOG_MAX_VOLUMES; s++)
    {
        float tNext = dist;
        for (int k = 0; k < hits; k++)
        {
            if (hitSegment[k].x > t)
                tNext = min(tNext, hitSegment[k].x);
            if (hitSegment[k].y > t)
                tNext = min(tNext, hitSegment[k].y);
        }

        vec4 medium = vec4(0.0); // xyz = color weighted by optical depth, w = optical depth
        if (uFogColorDensity.w > 0.0)
        {
            float tauGlobal = fogSegmentDepth(cameraPosition, rayDir, t, tNext, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y) * noiseFactor;
            medium += vec4(uFogColorDensity.xyz * tauGlobal, tauGlobal);
        }
        float tCenter = 0.5 * (t + tNext);
        for (int k = 0; k < hits; k++)
        {
            if (tCenter > hitSegment[k].x && tCenter < hitSegment[k].y)
            {
                float tau = hitExtinction[k] * (tNext - t);
                medium += vec4(uFogVolumes[hitIndex[k]].colorDensity.xyz * tau, tau);
            }
        }
        if (medium.w > 0.0)
        {
            float tr = exp(-medium.w);
            inscatter += transmittance * (1.0 - tr) * medium.xyz / medium.w;
            transmittance *= tr;
        }

        t = tNext;
        if (t >= dist)
            break;
    }
    return vec4(inscatter, transmittance);
}

// return value: 0 = clear sight, 1 = completely fogged
float fogAmount(vec3 cameraPosition, vec3 worldPosition)
{
    return 1.0 - fogIntegrate(cameraPosition, worldPosition).w;
}

// mixes fog into a lit surface color (call before splitting into color and bloom)
vec3 applyFog(vec3 color, vec3 worldPosition, vec3 cameraPosition)
{
    vec4 fog = fogIntegrate(cameraPosition, worldPosition);
    return color * fog.w + fog.xyz;
}
