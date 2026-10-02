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
    vec4 uFogColorDensity;   // xyz = color, w = thickness (0 = on fog)
    vec4 uFogHeightFalloff;  // x = base height, y = falloff to the top (0 = same from bottom to top), zw = not used (currently)
};

// return value: 0 = clear sight, 1 = completely fogged
float fogAmount(vec3 cameraPosition, vec3 worldPosition)
{
    if (uFogColorDensity.w <= 0.0)
        return 0.0;
    vec3 cameraToFragment = worldPosition - cameraPosition;
    float dist = length(cameraToFragment);
    vec3 rayDir = cameraToFragment / max(dist, 0.0001);
    float opticalDepth = fogOpticalDepthHeight(cameraPosition, rayDir, dist, uFogColorDensity.w, uFogHeightFalloff.x, uFogHeightFalloff.y);
    return fogAmountFromOpticalDepth(opticalDepth);
}

// Mischt die Nebelfarbe in eine (beleuchtete) Oberflaechenfarbe - fuer Forward-Shader.
// Muss VOR der Aufteilung in color/bloom aufgerufen werden.
vec3 applyFog(vec3 color, vec3 worldPosition, vec3 cameraPosition)
{
    return mix(color, uFogColorDensity.xyz, fogAmount(cameraPosition, worldPosition));
}
