#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;

layout(location = 0) out vec3 albedo; //rgb16f
layout(location = 1) out vec3 normal; //rgb16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec3 uColorTint;
uniform vec3 uColorMaterial;
uniform vec4 uColorEmissive;
uniform vec3 uMetallicRoughness;
uniform ivec2 uIdShadowCaster;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureRoughness;
uniform sampler2D uTextureMetallic;
uniform sampler2D uTextureEmissive;
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = 1 means: object has transparency!
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive
uniform int uTextureIsMetallicRoughnessCombined;

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
	normal = normalize(n);


	// ID & shadow caster behaviour:
	uint id = uIdShadowCaster.x;
	vec2 idAsRG = encode16BitUintTo8Bit(id);
	idShadowCaster = vec3(idAsRG, (uIdShadowCaster.y + 10) / 255.0);


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
			if(uUseTexturesMetallicRoughness.z > 0)
			{
				roughness = 1.0 - texture(uTextureRoughness, vTexture).r;
			}
			else
			{
				roughness = texture(uTextureRoughness, vTexture).r;
			}
		}
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
