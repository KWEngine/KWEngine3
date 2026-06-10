#version 400 core

in vec3 vImposterCenter;
in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in vec3 vColor;
in mat3 vTBN;

layout(location = 0) out vec3 albedo; // rgb8
layout(location = 1) out vec2 normal; // rg16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

uniform vec4 uColorTintEmissive;
uniform vec4 uPlayerPosShadowCaster;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform int uLightConfig;
uniform vec2 uRoughnessMetallic;
uniform int uMode;

vec2 encodeNormalToRG16F(vec3 n)
{
    n /= (abs(n.x) + abs(n.y) + abs(n.z));

    vec2 enc = (n.z >= 0.0)
        ? n.xy
        : (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);

    return enc;
}

vec3 decodeNormalFromRG16F(vec2 enc)
{
    vec2 f = enc;

    vec3 n = vec3(f.xy, 1.0 - abs(f.x) - abs(f.y));

    if (n.z < 0.0)
    {
        n.xy = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
    }

    return normalize(n);
}

vec2 octWrap(vec2 v)
{
    return (1.0 - abs(v.yx)) * sign(v.xy);
}

vec2 encodeNormal(vec3 n)
{
    n /= (abs(n.x) + abs(n.y) + abs(n.z));
    n.xy = n.z >= 0.0 ? n.xy : octWrap(n.xy);
    n.xy = n.xy * 0.5 + vec2(0.5);
    return n.xy;
}

vec2 encode16BitUintTo8Bit(uint value16)
{
    float low8  = float(value16 & 0xFFu);
    float high8 = float((value16 >> 8) & 0xFFu);
    return vec2(high8 / 255.0, low8 / 255.0);
}

// 4x4 bayer dithering for alpha cutout
float bayer4x4(vec2 fragCoord)
{
    int x = int(mod(fragCoord.x, 4.0));
    int y = int(mod(fragCoord.y, 4.0));
    int index = x + y * 4;

    const float bayer[16] = float[16](
         0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
        12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
         3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
        15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
    );

    return bayer[index];
}

void main()
{
    vec4 colorFromTexture = texture(uTextureAlbedo, vTexture);
    if(colorFromTexture.w < 0.6) discard;
    /*
    if (uMode > 0)
    {
        float alpha = colorFromTexture.a;

        const float alphaCutoff = 0.9;
        const float alphaSoftnessBase = 0.04;

        float alphaFw = fwidth(alpha);
        float alphaSoftness = max(alphaSoftnessBase, alphaFw * 1.5);

        float coverage = smoothstep(
            alphaCutoff - alphaSoftness,
            alphaCutoff + alphaSoftness,
            alpha
        );

        float dither = bayer4x4(gl_FragCoord.xy);

        if (coverage <= dither)
            discard;
    }
    */

    albedo = vColor * colorFromTexture.rgb * uColorTintEmissive.rgb;
    emissive = uColorTintEmissive.rgb * uColorTintEmissive.w * 0.5;

    if (uMode == 0)
    {
        normal = encodeNormalToRG16F(
            normalize(vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0))
        );
    }
    else
    {

        vec3 delta = normalize(vPosition.xyz - vImposterCenter);
        delta.y = 3.5;
        vec3 fakeNormal = normalize(delta);

        normal = encodeNormalToRG16F(fakeNormal);
    }

    metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, uRoughnessMetallic.x, 0.0);
    idShadowCaster = vec3(encode16BitUintTo8Bit(65535), (uLightConfig + 10) / 255.0);
}