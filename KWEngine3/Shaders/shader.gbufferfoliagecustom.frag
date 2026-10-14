#version 400 core

// G-buffer pass for FoliageObjectCustom instances (alpha tested imposter cards).

in vec3 vImposterCenter;
in vec4 vPosition;
in vec2 vTexture;
in vec3 vColor;

layout(location = 0) out vec3 albedo; // rgb8
layout(location = 1) out vec2 normal; // rg16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

uniform vec4 uColorTintEmissive;
uniform sampler2D uTextureAlbedo;
uniform int uLightConfig;
uniform vec2 uRoughnessMetallic;
uniform float uHue; // radians

// id 65535 marks foliage in the g-buffer (the lighting pass uses its own shading model for it)
const uint FOLIAGE_ID = 65535u;
const float ALPHA_CUTOFF = 0.6;
const int ALPHA_MIPSCALE_COUNT = 16;

// Box filtered mip maps change the alpha coverage, so cards would thin out (or clump) with distance.
// The CPU computes one alpha scale per mip level that keeps the coverage of the base level
// (HelperTexture.ComputeAlphaCoverageMipScales).
uniform float uAlphaMipScales[ALPHA_MIPSCALE_COUNT];

vec3 hueShift(vec3 color, float hue)
{
    const vec3 k = vec3(0.57735, 0.57735, 0.57735);
    float cosAngle = cos(hue);
    return vec3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
}

vec2 encodeNormalToRG16F(vec3 n)
{
    n /= (abs(n.x) + abs(n.y) + abs(n.z));

    vec2 enc = (n.z >= 0.0)
        ? n.xy
        : (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);

    return enc;
}

vec2 encode16BitUintTo8Bit(uint value16)
{
    float low8  = float(value16 & 0xFFu);
    float high8 = float((value16 >> 8) & 0xFFu);
    return vec2(high8 / 255.0, low8 / 255.0);
}

void main()
{
    vec4 colorFromTexture = texture(uTextureAlbedo, vTexture);
    float mipLevel = clamp(textureQueryLod(uTextureAlbedo, vTexture).x, 0.0, float(ALPHA_MIPSCALE_COUNT - 1));
    int mipLevel0 = int(floor(mipLevel));
    int mipLevel1 = min(mipLevel0 + 1, ALPHA_MIPSCALE_COUNT - 1);
    float alphaScale = mix(uAlphaMipScales[mipLevel0], uAlphaMipScales[mipLevel1], mipLevel - float(mipLevel0));
    float alpha = colorFromTexture.a * alphaScale;
    if(alpha < ALPHA_CUTOFF)
        discard;

    vec3 colorRGB = uHue != 0.0 ? hueShift(colorFromTexture.rgb, uHue) : colorFromTexture.rgb;
    albedo = vColor * colorRGB * uColorTintEmissive.rgb;
    emissive = uColorTintEmissive.rgb * uColorTintEmissive.w * 0.5;

    // rounded fake normal around the imposter center (bush-like shading)
    vec3 delta = vPosition.xyz - vImposterCenter;
    delta.y *= 0.25;
    vec3 fakeNormal = normalize(delta + vec3(0.0, 0.75, 0.0));
    normal = encodeNormalToRG16F(fakeNormal);

    metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, uRoughnessMetallic.x, 0.0);
    idShadowCaster = vec3(encode16BitUintTo8Bit(FOLIAGE_ID), (uLightConfig + 10) / 255.0);
}
