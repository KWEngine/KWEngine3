#version 400 core

in vec2 vTexture;
in vec2 vTextureSlope;
in vec3 vNormal;
in mat3 vTBN;
in vec3 vPos;
layout(location = 0) out vec3 albedo; //R11G11B10f
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec3 uColorTint;
uniform vec3 uColorMaterial;
uniform vec4 uColorEmissive;
uniform vec3 uMetallicRoughness;
uniform ivec2 uIdShadowCaster;

// REGULAR TEXTURES:
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureRoughness;
uniform sampler2D uTextureMetallic;
uniform sampler2D uTextureEmissive;
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = not used
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive
uniform vec4 uTextureTransform;

// SLOPE TEXTURES:
uniform sampler2D uTextureAlbedoSlope;
uniform sampler2D uTextureNormalSlope;
uniform sampler2D uTextureRoughnessSlope;
uniform sampler2D uTextureMetallicSlope;
uniform sampler2D uTextureEmissiveSlope;
uniform ivec3 uUseTexturesMetallicRoughnessSlope; // x = metallic, y = roughness, w = slopefactor * 100
uniform ivec3 uUseTexturesAlbedoNormalEmissiveSlope; // x = albedo, y = normal, z = emissive
uniform vec4 uTextureTransformSlope;

const float HALF_PI = 1.57079632680;

vec2 octWrap(vec2 v)
{
    //return (1.0 - abs(v.yx)) * (v.xy >= 0.0 ? 1.0 : -1.0);
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
	float dotNormal = dot(vNormal, vec3(0.0, 1.0, 0.0));
	float slopefactor = uUseTexturesMetallicRoughnessSlope.z * 0.001;
	int stepValue = 1 - int(step(slopefactor, dotNormal));

	vec4 texTransform = stepValue == 0 ? uTextureTransform : uTextureTransformSlope;
	vec3 texUseAlbedoNormalEmissive = stepValue == 0 ? uUseTexturesAlbedoNormalEmissive : uUseTexturesAlbedoNormalEmissiveSlope;
	vec3 texUseMetallicRoughness = stepValue == 0 ? uUseTexturesMetallicRoughness : uUseTexturesMetallicRoughnessSlope;


	vec3 blendfactors = getTriPlanarBlend(vNormal);
	vec2 xaxisUV = rotateUV(vPos.yz, HALF_PI);

	vec3 emissive;
	// Emissive color:
	if(texUseAlbedoNormalEmissive.z > 0)
	{
		vec3 xaxis = texture(stepValue == 0 ? uTextureEmissive : uTextureEmissiveSlope, xaxisUV * 0.125 * texTransform.x + texTransform.z).rgb* 5.0;
		vec3 yaxis = texture(stepValue == 0 ? uTextureEmissive : uTextureEmissiveSlope, vPos.xz * 0.125 * texTransform.y + texTransform.w).rgb* 5.0;
		vec3 zaxis = texture(stepValue == 0 ? uTextureEmissive : uTextureEmissiveSlope, vec2(vPos.x, -vPos.y) * 0.125 * texTransform.x + texTransform.z).rgb* 5.0;
		vec3 emissiveFromTexture = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z);
		emissive = emissiveFromTexture + uColorEmissive.xyz * uColorEmissive.w;
	}
	else
	{
		emissive = uColorEmissive.xyz * uColorEmissive.w;
	}

	// Albedo color:
	if(texUseAlbedoNormalEmissive.x > 0)
	{
		vec3 xaxis = texture(stepValue == 0 ? uTextureAlbedo : uTextureAlbedoSlope, xaxisUV * 0.125 * texTransform.x + texTransform.z).rgb;
		vec3 yaxis = texture(stepValue == 0 ? uTextureAlbedo : uTextureAlbedoSlope, vPos.xz * 0.125 * texTransform.y + texTransform.w).rgb;
		vec3 zaxis = texture(stepValue == 0 ? uTextureAlbedo : uTextureAlbedoSlope, vec2(vPos.x, -vPos.y) * 0.125 * texTransform.x + texTransform.z).rgb;
		albedo = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z) * uColorTint  + emissive;
	}
	else
	{
		albedo = uColorMaterial.xyz * uColorTint.xyz + emissive;
	}

	// Normals:
	vec3 n;
	if(texUseAlbedoNormalEmissive.y > 0)
	{
		vec3 xaxis = texture(stepValue == 0 ? uTextureNormal : uTextureNormalSlope, xaxisUV * 0.125 * texTransform.x + texTransform.z).rgb * 2.0 - 1.0;
		vec3 yaxis = texture(stepValue == 0 ? uTextureNormal : uTextureNormalSlope, vPos.xz * 0.125 * texTransform.y + texTransform.w).rgb * 2.0 - 1.0;
		vec3 zaxis = texture(stepValue == 0 ? uTextureNormal : uTextureNormalSlope, vec2(vPos.x, -vPos.y) * 0.125 * texTransform.x + texTransform.z).rgb * 2.0 - 1.0;
		n = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
		n = vTBN * n;
	}
	else
	{
		n = vNormal;
	}
	normal = normalize(n);
	
	uint id = uIdShadowCaster.x;
	vec2 idAsRG = encode16BitUintTo8Bit(id);
	idShadowCaster = vec3(idAsRG, (uIdShadowCaster.y + 10) / 255.0);

	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float roughness = uMetallicRoughness.y;
	bool roughnessThroughMetallic = false;
	if(texUseMetallicRoughness.x > 0)
	{
		float xaxis = texture(stepValue == 0 ? uTextureMetallic : uTextureMetallicSlope, xaxisUV * 0.125 * texTransform.x + texTransform.z).r;
		float yaxis = texture(stepValue == 0 ? uTextureMetallic : uTextureMetallicSlope, vPos.xz * 0.125 * texTransform.y + texTransform.w).r;
		float zaxis = texture(stepValue == 0 ? uTextureMetallic : uTextureMetallicSlope, vec2(vPos.x, -vPos.y) * 0.125 * texTransform.x + texTransform.z).r;
		metallic = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}
	if(texUseMetallicRoughness.y > 0)
	{
		float xaxis = texture(stepValue == 0 ? uTextureRoughness : uTextureRoughnessSlope, xaxisUV * 0.125 * texTransform.x + texTransform.z).r;
		float yaxis = texture(stepValue == 0 ? uTextureRoughness : uTextureRoughnessSlope, vPos.xz * 0.125 * texTransform.y + texTransform.w).r;
		float zaxis = texture(stepValue == 0 ? uTextureRoughness : uTextureRoughnessSlope, vec2(vPos.x, -vPos.y) * 0.125 * texTransform.x + texTransform.z).r;
		roughness = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
