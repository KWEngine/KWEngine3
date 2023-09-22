#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;

layout(location = 0) out vec4 positionId;
layout(location = 1) out vec4 albedo;
layout(location = 2) out vec4 normalDepth;
layout(location = 3) out vec3 metallicRoughnessMetallicType;
layout(location = 4) out vec4 emissive;

uniform vec3 uColorTint;
uniform vec3 uColorMaterial;
uniform vec4 uColorEmissive;
uniform vec3 uMetallicRoughness;
uniform int uId;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureRoughness;
uniform sampler2D uTextureMetallic;
uniform sampler2D uTextureEmissive;
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = 1 means: object has transparency!
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive

void main()
{
	positionId = vec4(vPosition.xyz, float(uId));

	// Albedo color:
	if(uUseTexturesAlbedoNormalEmissive.x > 0)
	{
		albedo = vec4(texture(uTextureAlbedo, vTexture).xyz * uColorTint, 1.0);
	}
	else
	{
		albedo = vec4(uColorMaterial.xyz * uColorTint.xyz, 1.0);
	}

	// Emissive color:
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		emissive = texture(uTextureEmissive, vTexture) + uColorEmissive;
	}
	else
	{
		emissive = uColorEmissive;
	}

	// Normals:
	vec3 normal;
	if(uUseTexturesAlbedoNormalEmissive.y > 0)
	{
		normal = vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0);
	}
	else
	{
		normal = vNormal;
	}
	normalDepth = vec4(normalize(normal), gl_FragCoord.z);

	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float roughness = uMetallicRoughness.y;
	bool roughnessThroughMetallic = false;
	if(uUseTexturesMetallicRoughness.x > 0)
	{
		metallic = texture(uTextureMetallic, vTexture).r;
	}
	if(uUseTexturesMetallicRoughness.y > 0)
	{
		roughness = texture(uTextureRoughness, vTexture).r;
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z);
}
