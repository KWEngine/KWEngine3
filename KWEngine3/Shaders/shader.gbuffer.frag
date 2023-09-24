#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType;
layout(location = 3) out float id;

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
	vec3 emissive;
	// Emissive color:
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		vec3 emissiveFromTexture = texture(uTextureEmissive, vTexture).xyz * 5.0;
		emissive = emissiveFromTexture + uColorEmissive.xyz * uColorEmissive.w;
	}
	else
	{
		emissive = uColorEmissive.xyz * uColorEmissive.w;
	}

	// Albedo color:
	if(uUseTexturesAlbedoNormalEmissive.x > 0)
	{
		albedo = texture(uTextureAlbedo, vTexture).xyz * uColorTint  + emissive;
	}
	else
	{
		albedo = uColorMaterial.xyz * uColorTint.xyz + emissive;
	}

	

	// Normals:
	vec3 n;
	if(uUseTexturesAlbedoNormalEmissive.y > 0)
	{
		n = vTBN * (texture(uTextureNormal, vTexture).xyz * 2.0 - 1.0);
	}
	else
	{
		n = vNormal;
	}
	normal = vec3(normalize(n));
	id = float(uId);

	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float roughness = uMetallicRoughness.y;
	bool roughnessThroughMetallic = false;
	if(uUseTexturesMetallicRoughness.x > 0) // x = metallic
	{
		if(uTextureIsMetallicRoughnessCombined > 0)
		{
			vec4 t = texture(uTextureMetallic, vTexture);
			metallic = t.b;  
			roughness = t.g; 
			roughnessThroughMetallic = true;
		}
		else
		{
			metallic = texture(uTextureMetallic, vTexture).r;
		}
	}
	if(uUseTexturesMetallicRoughness.y > 0) // y = roughness
	{
		if(!roughnessThroughMetallic)
		{
			roughness = texture(uTextureRoughness, vTexture).r;
		}
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
