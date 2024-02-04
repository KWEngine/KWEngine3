#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;
in vec3 vColor;

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType;
layout(location = 3) out ivec2 idShadowCaster;

uniform vec4 uColorTintEmissive;
uniform vec4 uPlayerPosShadowCaster;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;

void main()
{
	albedo = vec3(vColor); // * uColorTintEmissive.xyz * uColorTintEmissive.w);
	normal = vec3(0, 0, 0);
	metallicRoughnessMetallicType = vec3(0.0, 0.5, 0.0);
	idShadowCaster = ivec2(65535, 0);
}
