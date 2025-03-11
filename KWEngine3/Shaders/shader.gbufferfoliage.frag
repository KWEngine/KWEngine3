#version 400 core

in vec2 vTexture;
in vec3 vNormal;
in vec3 vColor;
in mat3 vTBN;

layout(location = 0) out vec3 albedo; //rgb16f
layout(location = 1) out vec3 normal; //rgb16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec4 uColorTintEmissive;
uniform vec4 uPlayerPosShadowCaster;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform int uLightConfig;
uniform vec2 uRoughnessMetallic;

/*
vec2 encodeNormal(vec3 normal) 
{
    vec2 projected = normal.xy / (1.0 + normal.z);

    vec2 encoded;
    encoded.x = (projected.x + 1.0) * 0.5;
    encoded.y = (projected.y + 1.0) * 0.5;

    return encoded;
}
*/

vec2 encodeNormal(vec3 normal) {
    float p = sqrt(normal.z * 8.0 + 8.0);
    return normal.xy / p + 0.5;
}

vec2 encode16BitUintTo8Bit(uint value16) 
{
    float low8 = float(value16 & 0xFFu); // Untere 8 Bit
    float high8 = float((value16 >> 8) & 0xFFu); // Obere 8 Bit
    return vec2(high8 / 255.0, low8 / 255.0);
}

void main()
{
	albedo = vColor * texture(uTextureAlbedo, vTexture).xyz * uColorTintEmissive.xyz * uColorTintEmissive.w;
	normal = normalize(vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0));
	metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, uRoughnessMetallic.x, 0.0);
	idShadowCaster = vec3(encode16BitUintTo8Bit(65535), (uLightConfig + 10) / 255.0);
}
