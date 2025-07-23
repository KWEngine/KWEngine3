#version 400 core

//in vec2 vTexture;
//in vec2 vTextureSlope;
in vec3 vNormal;
in mat3 vTBN;
in vec3 vPos;
in vec3 vTangentView;
in vec3 vTangentPosition;

layout(location = 0) out vec3 albedo; //R11G11B10f
layout(location = 1) out vec2 normal; // RG16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec3 uColorTint;
uniform vec3 uColorMaterial;
uniform vec4 uColorEmissive;
uniform vec3 uMetallicRoughness;
uniform ivec2 uIdShadowCaster;
uniform float uPOMScale;

// REGULAR TEXTURES:
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureRoughness;
uniform sampler2D uTextureMetallic;
uniform sampler2D uTextureEmissive;
uniform sampler2D uTextureHeight;
uniform ivec3 uUseTexturesMetallicRoughness; // x = metallic, y = roughness, z = smoothstep-width
uniform ivec3 uUseTexturesAlbedoNormalEmissive; // x = albedo, y = normal, z = emissive
uniform vec4 uTextureTransform;

// SLOPE TEXTURES:
uniform sampler2D uTextureAlbedoSlope;
uniform sampler2D uTextureNormalSlope;
uniform sampler2D uTextureRoughnessSlope;
uniform sampler2D uTextureMetallicSlope;
uniform sampler2D uTextureEmissiveSlope;
uniform ivec3 uUseTexturesMetallicRoughnessSlope; // x = metallic, y = roughness, z = slopefactor * 100
uniform ivec3 uUseTexturesAlbedoNormalEmissiveSlope; // x = albedo, y = normal, z = emissive
uniform vec4 uTextureTransformSlope;

const float HALF_PI = 1.57079632680;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{ 
	if(uPOMScale <= 0.0) return texCoords;

	const float minLayers = 4.0;
	const float maxLayers = 16.0;
	float numLayers = mix(maxLayers, minLayers, max(dot(vec3(0.0, 0.0, 1.0), viewDir), 0.0));  

    float layerDepth = 1.0 / numLayers;
    float currentLayerDepth = 0.0;
    vec2 P = viewDir.xy * uPOMScale; 
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
	vec3 viewDir = normalize(vTangentView - vTangentPosition);

	float dotNormal = dot(vNormal, vec3(0.0, 1.0, 0.0));
	float slopeSmoothingFactor = uUseTexturesMetallicRoughness.z * 0.001;
	float slopeBlendFactor = uUseTexturesMetallicRoughnessSlope.z * 0.001;
	float slopefactorL = max(0.0, slopeBlendFactor - slopeBlendFactor * slopeSmoothingFactor);
	float slopefactorH = min(1.0, slopeBlendFactor + slopeBlendFactor * slopeSmoothingFactor);
	float sstep = smoothstep(slopefactorL, slopefactorH, pow(dotNormal, 10.0));
	vec3 blendfactors = getTriPlanarBlend(vNormal);
	vec2 xaxisUV = rotateUV(vPos.yz, HALF_PI);

	vec2 uvX = xaxisUV * 0.125 * uTextureTransform.xy + uTextureTransform.zw;
	vec2 uvY = vPos.xz * 0.125 * uTextureTransform.xy + uTextureTransform.zw;
	vec2 uvZ = vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransform.xy + uTextureTransform.zw;
	uvX = ParallaxMapping(uvX, viewDir);
	uvY = ParallaxMapping(uvY, viewDir);
	uvZ = ParallaxMapping(uvZ, viewDir);

	vec2 uvSlopeX = xaxisUV * 0.125 * uTextureTransformSlope.xy + uTextureTransformSlope.zw;
	vec2 uvSlopeY = vPos.xz * 0.125 * uTextureTransformSlope.xy + uTextureTransformSlope.zw;
	vec2 uvSlopeZ = vec2(vPos.x, -vPos.y) * 0.125 * uTextureTransformSlope.xy + uTextureTransformSlope.zw;

	// =====================================

	// Emissive color:
	vec3 emissive = uColorEmissive.xyz * uColorEmissive.w;
	vec3 emissiveTex = vec3(0);
	vec3 emissiveSlope = vec3(0);
	if(uUseTexturesAlbedoNormalEmissive.z > 0)
	{
		vec3 xaxis = texture(uTextureEmissive, uvX).rgb* 5.0;
		vec3 yaxis = texture(uTextureEmissive, uvY).rgb* 5.0;
		vec3 zaxis = texture(uTextureEmissive, uvZ).rgb* 5.0;
		emissiveTex = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z);
	}

	if(uUseTexturesAlbedoNormalEmissiveSlope.z > 0)
	{
		vec3 xaxisSlope = texture(uTextureEmissiveSlope, uvSlopeX).rgb* 5.0;
		vec3 yaxisSlope = texture(uTextureEmissiveSlope, uvSlopeY).rgb* 5.0;
		vec3 zaxisSlope = texture(uTextureEmissiveSlope, uvSlopeZ).rgb* 5.0;
		emissiveSlope = (xaxisSlope * blendfactors.x + yaxisSlope * blendfactors.y + zaxisSlope * blendfactors.z);
	}
	else
	{
		emissiveSlope = emissiveTex;
	}
	
	emissive = emissiveTex * sstep + emissiveSlope * (1.0 - sstep)  + uColorEmissive.xyz * uColorEmissive.w;
	

	// Albedo color:
	albedo = uColorMaterial.xyz;
	vec3 albedoTex = albedo;
	vec3 albedoSlope = albedo;
	if(uUseTexturesAlbedoNormalEmissive.x > 0)
	{
		vec3 xaxis = texture(uTextureAlbedo, uvX).rgb;
		vec3 yaxis = texture(uTextureAlbedo, uvY).rgb;
		vec3 zaxis = texture(uTextureAlbedo, uvZ).rgb;
		albedoTex = (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z);
	}
	if(uUseTexturesAlbedoNormalEmissiveSlope.x > 0)
	{
		vec3 xaxisSlope = texture(uTextureAlbedoSlope, uvSlopeX).rgb;
		vec3 yaxisSlope = texture(uTextureAlbedoSlope, uvSlopeY).rgb;
		vec3 zaxisSlope = texture(uTextureAlbedoSlope, uvSlopeZ).rgb;
		albedoSlope = (xaxisSlope * blendfactors.x + yaxisSlope * blendfactors.y + zaxisSlope * blendfactors.z);
	}
	else
	{
		albedoSlope = albedoTex;
	}
	albedo = albedoTex * sstep + albedoSlope * (1.0 - sstep)  * uColorTint.xyz + emissive;

	// Normals:
	vec3 normalTex;
	vec3 normalSlope;
	if(uUseTexturesAlbedoNormalEmissive.y > 0)
	{
		vec3 xaxis = texture(uTextureNormal, uvX).rgb * 2.0 - 1.0;
		vec3 yaxis = texture(uTextureNormal, uvY).rgb * 2.0 - 1.0;
		vec3 zaxis = texture(uTextureNormal, uvZ).rgb * 2.0 - 1.0;
		normalTex = normalize(vTBN * (xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z));
	}
	else
	{
		normalTex = vNormal;
	}	
	if(uUseTexturesAlbedoNormalEmissiveSlope.y > 0)
	{
		vec3 xaxisSlope = texture(uTextureNormalSlope, uvSlopeX).rgb * 2.0 - 1.0;
		vec3 yaxisSlope = texture(uTextureNormalSlope, uvSlopeY).rgb * 2.0 - 1.0;
		vec3 zaxisSlope = texture(uTextureNormalSlope, uvSlopeZ).rgb * 2.0 - 1.0;
		normalSlope = normalize(vTBN * (xaxisSlope * blendfactors.x + yaxisSlope * blendfactors.y + zaxisSlope * blendfactors.z));
	}
	else
	{
		normalSlope = normalTex;
	}
	normal = encodeNormalToRG16F(normalTex * sstep + normalSlope * (1.0 - sstep));


	// MISC:
	uint id = uIdShadowCaster.x;
	vec2 idAsRG = encode16BitUintTo8Bit(id);
	idShadowCaster = vec3(idAsRG, (uIdShadowCaster.y + 10) / 255.0);

	// PBR:
	//Metallic/Roughness
	float metallic = uMetallicRoughness.x;
	float metallicTex = metallic;
	float metallicSlope = metallic;
	if(uUseTexturesMetallicRoughness.x > 0)
	{
		float xaxis = texture(uTextureMetallic, uvX).r;
		float yaxis = texture(uTextureMetallic, uvY).r;
		float zaxis = texture(uTextureMetallic, uvZ).r;
		metallicTex = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}
	if(uUseTexturesMetallicRoughnessSlope.x > 0)
	{
		float xaxisSlope = texture(uTextureMetallicSlope, uvSlopeX).r;
		float yaxisSlope = texture(uTextureMetallicSlope, uvSlopeY).r;
		float zaxisSlope = texture(uTextureMetallicSlope, uvSlopeZ).r;
		metallicSlope = xaxisSlope * blendfactors.x + yaxisSlope * blendfactors.y + zaxisSlope * blendfactors.z;
	}
	else
	{
		metallicSlope = metallicTex;
	}
	metallic = metallicTex * sstep + metallicSlope * (1.0 - sstep);





	float roughness = uMetallicRoughness.y;
	float roughnessTex = roughness;
	float roughnessSlope = roughness;
	if(uUseTexturesMetallicRoughness.y > 0)
	{
		float xaxis = texture(uTextureRoughness, uvX).r;
		float yaxis = texture(uTextureRoughness, uvY).r;
		float zaxis = texture(uTextureRoughness, uvZ).r;
		roughnessTex = xaxis * blendfactors.x + yaxis * blendfactors.y + zaxis * blendfactors.z;
	}
	if(uUseTexturesMetallicRoughnessSlope.y > 0)
	{
		float xaxisSlope = texture(uTextureRoughnessSlope, uvSlopeX).r;
		float yaxisSlope = texture(uTextureRoughnessSlope, uvSlopeY).r;
		float zaxisSlope = texture(uTextureRoughnessSlope, uvSlopeZ).r;
		roughnessSlope = xaxisSlope * blendfactors.x + yaxisSlope * blendfactors.y + zaxisSlope * blendfactors.z;
	}
	else
	{
		roughnessSlope = roughnessTex;
	}
	roughness = roughnessTex * sstep + roughnessSlope * (1.0 - sstep);

	metallicRoughnessMetallicType = vec3(metallic, max(roughness, 0.00001), uMetallicRoughness.z / 9.0);
}
