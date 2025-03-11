#version 400 core

in vec2 vTexture;
in vec3 vNormal;
in mat3 vTBN;
in vec3 vPos;

layout(location = 0) out vec3 albedo; //rgb16f
layout(location = 1) out vec2 normal; //rg8
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
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = 1 means: object has transparency (not used yet)!
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive
uniform vec4 uTextureTransform;

const float HALF_PI = 1.57079632680;

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

vec3 getTriPlanarBlend(vec3 n){
	vec3 blending = abs(n);
	blending = normalize(max(blending, 0.00001));
	float b = (blending.x + blending.y + blending.z);
	blending /= vec3(b, b, b);
	return blending;
}

vec2 rotateUV(vec2 uv, float rotation)
{
    float mid = 0.0;
    float cosAngle = cos(rotation);
    float sinAngle = sin(rotation);
    return vec2(
        cosAngle * (uv.x - mid) + sinAngle * (uv.y - mid) + mid,
        cosAngle * (uv.y - mid) - sinAngle * (uv.x - mid) + mid
    );
}

void main()
{
	vec3 blendfactors = getTriPlanarBlend(vNormal);
	vec2 xaxisUV = rotateUV(vPos.yz, HALF_PI);

	vec3 emissive;
	// Emissive color:
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		vec3 xaxis = texture(uTextureAlbedo, xaxisUV * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb* 5.0;
		vec3 yaxis = texture(uTextureAlbedo, vPos.xz * 0.125 * uTextureTransform.y + uTextureTransform.w).rgb* 5.0;
		vec3 zaxis = texture(uTextureAlbedo, vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb* 5.0;
		vec3 emissiveFromTexture = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z);
		emissive = emissiveFromTexture + uColorEmissive.xyz * uColorEmissive.w;
	}
	else
	{
		emissive = uColorEmissive.xyz * uColorEmissive.w;
	}

	// Albedo color:
	if(uUseTexturesAlbedoNormalEmissive.x > 0)
	{
		vec3 xaxis = texture(uTextureAlbedo, xaxisUV * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb;
		vec3 yaxis = texture(uTextureAlbedo, vPos.xz * 0.125 * uTextureTransform.y + uTextureTransform.w).rgb;
		vec3 zaxis = texture(uTextureAlbedo, vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb;
		albedo = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z) * uColorTint  + emissive;
	}
	else
	{
		albedo = uColorMaterial.xyz * uColorTint.xyz + emissive;
	}

	// Normals:
	vec3 n;
	if(uUseTexturesAlbedoNormalEmissive.y > 0)
	{
		vec3 xaxis = texture(uTextureNormal, xaxisUV * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb * 2.0 - 1.0;
		vec3 yaxis = texture(uTextureNormal, vPos.xz * 0.125 * uTextureTransform.y + uTextureTransform.w).rgb * 2.0 - 1.0;
		vec3 zaxis = texture(uTextureNormal, vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.x + uTextureTransform.z).rgb * 2.0 - 1.0;
		n = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
		n = vTBN * n;
	}
	else
	{
		n = vNormal;
	}
	normal = encodeNormal(normalize(n));
	
	uint id = uIdShadowCaster.x;
	vec2 idAsRG = encode16BitUintTo8Bit(id);
	idShadowCaster = vec3(idAsRG, (uIdShadowCaster.y + 10) / 255.0);

	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float roughness = uMetallicRoughness.y;
	bool roughnessThroughMetallic = false;
	if(uUseTexturesMetallicRoughness.x > 0)
	{
		float xaxis = texture(uTextureMetallic, xaxisUV * 0.125 * uTextureTransform.x + uTextureTransform.z).r;
		float yaxis = texture(uTextureMetallic, vPos.xz * 0.125 * uTextureTransform.y + uTextureTransform.w).r;
		float zaxis = texture(uTextureMetallic, vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.x + uTextureTransform.z).r;
		metallic = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}
	if(uUseTexturesMetallicRoughness.y > 0)
	{
		float xaxis = texture(uTextureRoughness, xaxisUV * 0.125 * uTextureTransform.x + uTextureTransform.z).r;
		float yaxis = texture(uTextureRoughness, vPos.xz * 0.125 * uTextureTransform.y + uTextureTransform.w).r;
		float zaxis = texture(uTextureRoughness, vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.x + uTextureTransform.z).r;
		roughness = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
