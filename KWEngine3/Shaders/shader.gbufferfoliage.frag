#version 400 core

in vec2 vTexture;
in vec3 vNormal;
in vec3 vColor;
in mat3 vTBN;

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType;
layout(location = 3) out ivec2 idShadowCaster;

uniform vec4 uColorTintEmissive;
uniform vec4 uPlayerPosShadowCaster;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform int uLightConfig;

void main()
{
	albedo = vColor * texture(uTextureAlbedo, vTexture).xyz * uColorTintEmissive.xyz * uColorTintEmissive.w;
	normal = normalize(vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0));
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
	idShadowCaster = ivec2(65535, uLightConfig);
}
