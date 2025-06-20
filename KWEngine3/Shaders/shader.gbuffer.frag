#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;

layout(location = 0) out vec3 albedo; //R11G11B10f
layout(location = 1) out vec2 normal; // rg16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec4 uColorTint; // 2025-06-13 vec3 -> vec4 for hue
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

vec2 encodeNormalToRG16F(vec3 n) {
    n /= (abs(n.x) + abs(n.y) + abs(n.z));

    vec2 enc = (n.z >= 0.0) 
        ? n.xy 
        : (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);

    return enc;
}

vec3 decodeNormalFromRG16F(vec2 enc) {
    vec2 f = enc;

    vec3 n = vec3(f.xy, 1.0 - abs(f.x) - abs(f.y));

    if (n.z < 0.0) {
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
    float low8 = float(value16 & 0xFFu); // Untere 8 Bit
    float high8 = float((value16 >> 8) & 0xFFu); // Obere 8 Bit
    return vec2(high8 / 255.0, low8 / 255.0);
}

// 2025-06-13: added hue
vec3 hueShift(vec3 color, float hue)
{
	const vec3 k = vec3(0.57735, 0.57735, 0.57735);
	float cosAngle = cos(hue);
	return vec3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
}

void main()
{
	vec3 emissive;
	// Emissive color:
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		vec4 tmp = texture(uTextureEmissive, vTexture);
		vec3 emissiveFromTexture = hueShift(tmp.xyz, uColorTint.w) * tmp.w;
		emissive = (emissiveFromTexture + uColorEmissive.xyz) * uColorEmissive.w;
	}
	else
	{
		emissive = uColorEmissive.xyz * uColorEmissive.w;
	}

	// Albedo color:
	if(uUseTexturesAlbedoNormalEmissive.x > 0)
	{
		albedo = hueShift(texture(uTextureAlbedo, vTexture).xyz, uColorTint.w) * uColorTint.xyz  + emissive;
	}
	else
	{
		albedo = hueShift(uColorMaterial.xyz, uColorTint.w) * uColorTint.xyz + emissive;
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
	normal = encodeNormalToRG16F(normalize(n));


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
