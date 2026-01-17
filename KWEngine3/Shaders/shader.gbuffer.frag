#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;
in vec3 vTangentView;
in vec3 vTangentPosition;

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
uniform sampler2D uTextureHeight;
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = 1 means: object has transparency!
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive
uniform int uTextureIsMetallicRoughnessCombined;
uniform vec4 uCameraPosition;

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

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{ 
	if(uCameraPosition.w <= 0.0) return texCoords;

	const float minLayers = 4.0;
	const float maxLayers = 16.0;
	float numLayers = mix(maxLayers, minLayers, max(dot(vec3(0.0, 0.0, 1.0), viewDir), 0.0));  

    float layerDepth = 1.0 / numLayers;
    float currentLayerDepth = 0.0;
    vec2 P = viewDir.xy * uCameraPosition.w; 
    vec2 deltaTexCoords = P / numLayers;

	vec2  currentTexCoords     = texCoords;
	float currentDepthMapValue = 1.0 - texture(uTextureHeight, currentTexCoords).r;
  
	while(currentLayerDepth < currentDepthMapValue)
	{
		currentTexCoords -= deltaTexCoords;
		currentDepthMapValue = 1.0 - texture(uTextureHeight, currentTexCoords).r;  
		currentLayerDepth += layerDepth;  
	}

	vec2 prevTexCoords = currentTexCoords + deltaTexCoords;

	float afterDepth  = currentDepthMapValue - currentLayerDepth;
	float beforeDepth = 1.0 - texture(uTextureHeight, prevTexCoords).r - currentLayerDepth + layerDepth;
 
	float weight = afterDepth / (afterDepth - beforeDepth);
	vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

	return finalTexCoords;
	
} 

void main()
{
	vec3 viewDir = normalize(vTangentView - vTangentPosition);
	vec2 vTexture2 = ParallaxMapping(vTexture, viewDir);

	vec3 emissive;
	// Emissive color:
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		vec4 tmp = texture(uTextureEmissive, vTexture2);
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
		albedo = hueShift(texture(uTextureAlbedo, vTexture2).xyz, uColorTint.w) * uColorTint.xyz  + emissive;
	}
	else
	{
		albedo = hueShift(uColorMaterial.xyz, uColorTint.w) * uColorTint.xyz + emissive;
	}

	

	// Normals:
	vec3 n;
	if(uUseTexturesAlbedoNormalEmissive.y > 0)
	{
		n = vTBN * (texture(uTextureNormal, vTexture2).xyz * 2.0 - 1.0);
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
	vec4 textureRoughness =  texture(uTextureRoughness, vTexture2);
	vec4 textureMetallic = texture(uTextureMetallic, vTexture2);
	if(uUseTexturesMetallicRoughness.x > 0) // x = metallic
	{
		if(uTextureIsMetallicRoughnessCombined > 0)
		{
			metallic = textureMetallic.b;  
			roughness = textureMetallic.g; 
			roughnessThroughMetallic = true;
		}
		else
		{
			metallic = textureMetallic.r;
		}
	}
	if(uUseTexturesMetallicRoughness.y > 0) // y = roughness
	{
		if(!roughnessThroughMetallic)
		{
			if(uUseTexturesMetallicRoughness.z > 0)
			{
				roughness = 1.0 - textureRoughness.r;
			}
			else
			{
				roughness = textureRoughness.r;
			}
		}
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
