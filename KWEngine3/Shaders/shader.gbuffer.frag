#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;

layout(location = 0) out vec4 positionDepth;
layout(location = 1) out vec4 albedo;
layout(location = 2) out vec4 normalId;
layout(location = 3) out vec4 csDepthMetallicRoughnessMetallicType;
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
uniform int uTextureIsMetallicRoughnessCombined;

void main()
{
	positionDepth = vPosition;

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
	normalId = vec4(normalize(normal), float(uId));

	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float roughness = uMetallicRoughness.y;
	bool roughnessThroughMetallic = false;
	if(uUseTexturesMetallicRoughness.x > 0)
	{
		if(uTextureIsMetallicRoughnessCombined > 0)
		{
			vec4 t = texture(uTextureMetallic, vTexture);
			metallic = t.b;  //TODO
			roughness = t.g; //TODO
			roughnessThroughMetallic = true;
		}
		else
		{
			metallic = texture(uTextureMetallic, vTexture).r;
		}
	}
	if(uUseTexturesMetallicRoughness.y > 0)
	{
		if(!roughnessThroughMetallic)
		{
			roughness = texture(uTextureRoughness, vTexture).r;
		}
	}

	csDepthMetallicRoughnessMetallicType = vec4(gl_FragCoord.z, metallic, max(roughness, 0.00001), uMetallicRoughness.z);
}
