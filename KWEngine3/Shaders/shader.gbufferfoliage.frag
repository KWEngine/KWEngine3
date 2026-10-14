#version 400 core

in vec2 vTexture;
in vec3 vColor;
in mat3 vTBN;
in float vNormalBend;
in vec3 vNormalBendTarget;

layout(location = 0) out vec3 albedo; // rgb8
layout(location = 1) out vec2 normal; // rg16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

uniform vec4 uColorTintEmissive;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform int uLightConfig;
uniform vec2 uRoughnessMetallic;
uniform float uHue; // radians

// id 65535 marks foliage in the g-buffer (the lighting pass uses its own shading model for it)
const uint FOLIAGE_ID = 65535u;

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
    vec3 colorFromTexture = texture(uTextureAlbedo, vTexture).rgb;
    if(uHue != 0.0)
        colorFromTexture = hueShift(colorFromTexture, uHue);

    albedo = vColor * colorFromTexture * uColorTintEmissive.rgb;
    emissive = uColorTintEmissive.rgb * uColorTintEmissive.w * 0.5;

    vec3 n = normalize(vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0));
    n = normalize(mix(n, vNormalBendTarget, vNormalBend) + vec3(0.0, 0.0001, 0.0));
    normal = encodeNormalToRG16F(n);

    metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, uRoughnessMetallic.x, 0.0);
    idShadowCaster = vec3(encode16BitUintTo8Bit(FOLIAGE_ID), (uLightConfig + 10) / 255.0);
}
