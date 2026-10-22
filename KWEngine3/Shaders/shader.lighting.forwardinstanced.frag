#version 400 core

in vec4 vPosition;
in vec2 vTexture;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBiTangent;
in mat3 vTBN;
in vec4 vShadowCoord[3];
in vec4 vShadowCoordOuter[3];
in vec3 vTangentView;
in vec3 vTangentPosition;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

uniform sampler2DArray uShadowMap[3];
uniform samplerCubeArray uShadowMapCube[3];
uniform float uLights[850];
uniform int uLightCount;
uniform vec3 uColorAmbient;
uniform int uTextureIsMetallicRoughnessCombined;
uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureMetallic;
uniform sampler2D uTextureRoughness;
uniform sampler2D uTextureEmissive;
uniform sampler2D uTextureTransparency;
uniform sampler2D uTextureHeight;
uniform ivec3 uUseTexturesAlbedoNormalEmissive;
uniform ivec3 uUseTexturesMetallicRoughness;

uniform samplerCube uTextureSkybox;
uniform mat3 uTextureSkyboxRotation;
uniform sampler2D uTextureBackground;
uniform ivec4 uUseTextureReflectionQuality;

uniform vec3 uMetallicRoughness;
uniform vec4 uColorEmissive;
uniform vec4 uColorMaterial;
uniform vec4 uColorTint;
uniform int uMetallicType;
uniform int uShadowCaster;

uniform vec4 uCameraPosition;

const float PI = 3.141593;
const float PI2 = 0.5 / PI;
const float ninetydegrees = 1.5708;
const mat4 quantizationMatrix3Inv = mat4(
										0.2227744146, 0.1549679261, 0.1451988946, 0.163127443,
										0.0771972861, 0.1394629426, 0.2120202157, 0.2591432266,
										0.7926986636, 0.7963415838, 0.7258694464, 0.6539092497,
										0.0319417555, -0.1722823173, -0.2758014811f, -0.3376131734
										);
const float offsetZero = 0.035955884801;
const vec2 fragOffsets[] = vec2[5](vec2(0, 0), vec2(-1, -1), vec2(1, -1), vec2(1, 1), vec2(-1, 1));
const float fragOffsetsWeights[] = float[5](0.4, 0.15, 0.15, 0.15, 0.15);
const float fragOffsetsWeights2[] = float[5](1.0, 5.0, 5.0, 5.0, 5.0);
/*
Default             = 0 =>  0.04, 0.04, 0.04
PlasticOrGlassLow   = 1 =>  0.03, 0.03, 0.03
PlasticOrGlassHigh  = 2 =>  0.05, 0.05, 0.05
Diamond             = 3 =>  0.17, 0.17, 0.17    
Iron                = 4 =>  0.56, 0.57, 0.58
Copper              = 5 =>  0.95, 0.64, 0.54
Gold                = 6 =>  1.00, 0.71, 0.29
Aluminium           = 7 =>  0.91, 0.92, 0.92
Silver              = 8 =>  0.95, 0.93, 0.88
*/
const vec3 metallicF0Values[9] = vec3[](
    vec3(0.04, 0.04, 0.04),
    vec3(0.03,0.03,0.03),
    vec3(0.05, 0.05, 0.05),
    vec3(0.17, 0.17, 0.17),
    vec3(0.56, 0.57, 0.58),
    vec3(0.95, 0.64, 0.54),
    vec3(1.00, 0.71, 0.29),
    vec3(0.91, 0.92, 0.92),
    vec3(0.95, 0.93, 0.88)
    );

vec2 vTexture2;

vec3 sampleFromEquirectangular(vec3 worldPosition, vec3 normal, float mipMapLevel)
{
    vec3 R = normalize(reflect(worldPosition - uCameraPosition.xyz, normal));
    vec2 uv;
    uv.x = atan( -R.z, -R.x ) * PI2 + 0.5;
    uv.y = 1.0 - (R.y * 0.5 + 0.5);

    return textureLod(uTextureBackground, uv, mipMapLevel).xyz;
}

vec3 getF0(int type)
{
    return metallicF0Values[type];
}

vec3 getReflectionColor(vec3 fragmentToCamera, vec3 N, float roughness, vec3 fragPosWorld)
{
	float mipMapLevel = 0.0;
	vec3 refl = vec3(1.0);
    // x = type, y = mipmaplevels
	if(uUseTextureReflectionQuality.x == 1) //2 = equi, 1 = cubemap 
	{
		vec3 reflectedCameraSurfaceNormal =  reflect(-fragmentToCamera, N) * uTextureSkyboxRotation;

		int mipMapLevels = uUseTextureReflectionQuality.y;
		mipMapLevel = roughness * mipMapLevels;
		refl = textureLod(uTextureSkybox, reflectedCameraSurfaceNormal, mipMapLevel).xyz * (float(uUseTextureReflectionQuality.z) / 1000.0);
	}
    else if(uUseTextureReflectionQuality.x == 2)
    {
        vec3 reflectedCameraSurfaceNormal =  reflect(-fragmentToCamera, N) * uTextureSkyboxRotation;

		int mipMapLevels = uUseTextureReflectionQuality.y;
		mipMapLevel = roughness * mipMapLevels;
		refl = sampleFromEquirectangular(fragPosWorld, reflectedCameraSurfaceNormal, mipMapLevel);
    }
	else if(uUseTextureReflectionQuality.x < 0)
	{
		vec3 reflectedCameraSurfaceNormal = reflect(-fragmentToCamera, N);
		vec2 coordinates = (reflectedCameraSurfaceNormal.xy + 1.0) / 2.0;
		coordinates.y = -coordinates.y;

		int mipMapLevels = uUseTextureReflectionQuality.y;
		mipMapLevel = roughness * mipMapLevels;

		refl = textureLod(uTextureBackground, coordinates, mipMapLevel).xyz * (float(uUseTextureReflectionQuality.z) / 1000.0);
	}
	return refl;
}

float calculateShadow(vec4 bQuantized, float fragmentDepth, float alpha, float hardness)
{
    // empty texel (nothing rendered there) is always lit: old clear value (1,1,1,1) or quantized moments of depth 1.0
    if(bQuantized.x >= 0.9999 && bQuantized.y >= 0.997)
        return 1.0;

	bQuantized.x -= offsetZero;
	vec4 _4Moments = quantizationMatrix3Inv * bQuantized;

	vec4 b = mix(_4Moments, vec4(0.5, 0.5, 0.5, 0.5), alpha);
	vec3 z = vec3(0);
	z.x = fragmentDepth - hardness;

	float L32D22 = -b.x * b.y + b.z;				
	float D22 = -b.x * b.x + b.y;					
	float SquaredDepthVariance = -b.y * b.y + b.w;	
	float D33D22 = dot(vec2(SquaredDepthVariance,-L32D22), vec2(D22, L32D22));
	float InvD22 = 1.0 / D22;
	float L32 = L32D22 * InvD22;
	vec3 c = vec3(1.0, z.x, z.x*z.x);
	c.y -= b.x;
	c.z -= b.y + L32 * c.y;
	c.y *= InvD22;
	c.z *= D22 / D33D22;
	c.y -= L32*c.z;
	c.x -= dot(c.yz,b.xy);

	// solve the quadr...
	float p = c.y / c.z;
	float q = c.x / c.z;
	float r = sqrt((p * p * 0.25) - q);
	z.y = -(p * 0.5f) - r;
	z.z = -(p * 0.5f) + r;
	vec4 Switch= (z.z < z.x) ? vec4(z.y, z.x, 1.0, 1.0) : ((z.y < z.x) ? vec4(z.x, z.y, 0.0, 1.0) : vec4(0.0, 0.0, 0.0, 0.0));
	float Quotient = (Switch.x * z.z - b.x * (Switch.x + z.z) + b.y) / ((z.z - Switch.y) * (z.x - z.y));
	float shadowValue = Switch.z + Switch.w * Quotient;
	return 1.0 - clamp(shadowValue, 0.0, 1.0);
}

// returns the two axes of the cube face that the direction d points to, scaled to the size of one texel on that face
void getCubeFaceTexelAxes(vec3 d, float faceSize, out vec3 axisU, out vec3 axisV)
{
    vec3 a = abs(d);
    float majorAxis;
    if(a.x >= a.y && a.x >= a.z)
    {
        majorAxis = a.x;
        axisU = vec3(0.0, 1.0, 0.0);
        axisV = vec3(0.0, 0.0, 1.0);
    }
    else if(a.y >= a.z)
    {
        majorAxis = a.y;
        axisU = vec3(1.0, 0.0, 0.0);
        axisV = vec3(0.0, 0.0, 1.0);
    }
    else
    {
        majorAxis = a.z;
        axisU = vec3(1.0, 0.0, 0.0);
        axisV = vec3(0.0, 1.0, 0.0);
    }
    float texelSize = 2.0 * majorAxis / faceSize;
    axisU *= texelSize;
    axisV *= texelSize;
}

float calculateShadowCube(int index, vec3 lightPos, vec3 fragPos, vec2 lightNearFar, float bias, float hardness)
{
	vec3 lightToFrag = fragPos - lightPos;
	float currentDepth = length(lightToFrag);
	float fragmentDepthLinearized = (currentDepth - lightNearFar.x) / (lightNearFar.y - lightNearFar.x);
    if(uUseTextureReflectionQuality.w > 0)
    {
        // high quality: 5 samples one texel apart on the cube face, the moments are averaged and solved only once
        vec3 axisU;
        vec3 axisV;
        getCubeFaceTexelAxes(lightToFrag, float(textureSize(uShadowMapCube[index], 0).x), axisU, axisV);
        vec4 sampledMomentsSum = vec4(0.0);
        for(int i = 0; i < 5; i++)
        {
            vec3 sampleDirection = lightToFrag + axisU * fragOffsets[i].x + axisV * fragOffsets[i].y;
            sampledMomentsSum += textureLod(uShadowMapCube[index], vec4(sampleDirection, 0.0), 0.0) * fragOffsetsWeights[i];
        }
        return calculateShadow(sampledMomentsSum, fragmentDepthLinearized, bias, hardness);
    }
    vec4 sampledDepthMSM = texture(uShadowMapCube[index], vec4(lightToFrag, 0.0));
	return calculateShadow(sampledDepthMSM, fragmentDepthLinearized, bias, hardness);
}  

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}  

float GeometrySchlickGGX(float NdotV, float k)
{
    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return nom / denom;
}
  
float GeometrySmith(vec3 N, vec3 V, vec3 L, float k)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx1 = GeometrySchlickGGX(NdotV, k);
    float ggx2 = GeometrySchlickGGX(NdotL, k);
	
    return ggx1 * ggx2;
}

float DistributionGGX(vec3 N, vec3 H, float a)
{
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float nom    = a2;
    float denom  = (NdotH2 * (a2 - 1.0) + 1.0);
    denom        = PI * denom * denom;
	
    return nom / denom;
}

vec4 getAlphaTexture()
{
    return texture(uTextureTransparency, vTexture2);
}

vec4 getPBR()
{
    vec4 specularMetallicRoughnessOcclusion = vec4(0.0, uMetallicRoughness.x, clamp(uMetallicRoughness.y, 0.0001, 1.0), 1.0);
    vec4 textureMetallic = texture(uTextureMetallic, vTexture2);
    if(uUseTexturesMetallicRoughness.x > 0) // check metallic
    {
        specularMetallicRoughnessOcclusion.y = textureMetallic.b; // blue channel for metallic
    }
    if(uUseTexturesMetallicRoughness.y > 0)
    {
        if(uTextureIsMetallicRoughnessCombined > 0)
        {
            specularMetallicRoughnessOcclusion.z = clamp(textureMetallic.g, 0.0001, 1.0);
        }
        else
        {
            if(uUseTexturesMetallicRoughness.z > 0)
            {
                specularMetallicRoughnessOcclusion.z = clamp(1.0 - texture(uTextureRoughness, vTexture2).r, 0.0001, 1.0);
            }
            else
            {
                specularMetallicRoughnessOcclusion.z = clamp(texture(uTextureRoughness, vTexture2).r, 0.0001, 1.0);
            }
        }
    }
    return specularMetallicRoughnessOcclusion;
}

vec4 getFragmentPositionAndDepth()
{
    return vec4(vPosition.xyz, 0.0);
}

// 2025-06-13: added hue
vec3 hueShift(vec3 color, float hue)
{
	const vec3 k = vec3(0.57735, 0.57735, 0.57735);
	float cosAngle = cos(hue);
	return vec3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
}

vec4 getAlbedo()
{
    vec4 albedo;
    if(uUseTexturesAlbedoNormalEmissive.x > 0)
    {
        vec4 tmp = texture(uTextureAlbedo, vTexture2);
        albedo = vec4(hueShift(tmp.xyz, uMetallicRoughness.z), tmp.w * uColorMaterial.w) * uColorTint;
    }
    else
    {
        
        albedo = vec4(hueShift(uColorMaterial.xyz, uMetallicRoughness.z), uColorMaterial.w) * uColorTint;
    }

    vec4 alpha = getAlphaTexture();
    albedo.w = min(albedo.w, alpha.w);

    return albedo;
}

vec3 getEmissive()
{
    if(uUseTexturesAlbedoNormalEmissive.z > 0)
    {
        vec4 tmp = texture(uTextureEmissive, vTexture2);
		vec3 emissiveFromTexture = hueShift(tmp.xyz, uMetallicRoughness.z) * uColorEmissive.w * tmp.w;
		return emissiveFromTexture;
    }
    else
    {
        return hueShift(uColorEmissive.xyz, uMetallicRoughness.z) * uColorEmissive.w;
    }
}

vec4 getNormalId()
{
    vec4 normalId = vec4(vNormal, 0.0);
    if(uUseTexturesAlbedoNormalEmissive.y > 0)
    {
        normalId.xyz = normalize(vTBN * (texture(uTextureNormal, vTexture2).xyz * 2.0 - 1.0));
    }

    return normalId;
}

vec2 getShadowMapVisiblity(vec3 texCoordInner, vec3 texCoordOuter)
{
    float distLeft = texCoordInner.x;        // Distanz zum linken Rand (0)
    float distRight = 1.0 - texCoordInner.x; // Distanz zum rechten Rand (1)
    float distBottom = texCoordInner.y;      // Distanz zum unteren Rand (0)
    float distTop = 1.0 - texCoordInner.y;   // Distanz zum oberen Rand (1)

    float distLeftOuter = texCoordOuter.x;        // Distanz zum linken Rand (0)
    float distRightOuter = 1.0 - texCoordOuter.x; // Distanz zum rechten Rand (1)
    float distBottomOuter = texCoordOuter.y;      // Distanz zum unteren Rand (0)
    float distTopOuter = 1.0 - texCoordOuter.y;   // Distanz zum oberen Rand (1)

    float minDistInner = min(min(distLeft, distRight), min(distBottom, distTop));
    float minDistOuter = min(min(distLeftOuter, distRightOuter), min(distBottomOuter, distTopOuter));

    float fade;
    float sampleLayer = 0.0;
    if(minDistInner < 0.05)
    {
        fade = smoothstep(0.0, 0.05, minDistOuter);
        sampleLayer = 1.0;
    }
    else
    {
        fade = smoothstep(0.0, 0.05, minDistInner);
    }

    return vec2(1.0 - fade, sampleLayer);
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

float fogDensityAtHeight(float y, float baseDensity, float baseHeight, float falloff)
{
    return baseDensity * exp(clamp(-falloff * (y - baseHeight), -80.0, 80.0));
}

float fogOpticalDepthHeight(vec3 rayOrigin, vec3 rayDir, float dist, float baseDensity, float baseHeight, float falloff)
{
    // density(t) = density(0) * exp(-k * t)
    float k = falloff * rayDir.y;
    if (abs(k * dist) > 0.001)
    {
        // (density(0) - density(dist)) / k
        float d0 = fogDensityAtHeight(rayOrigin.y, baseDensity, baseHeight, falloff);
        float d1 = fogDensityAtHeight(rayOrigin.y + rayDir.y * dist, baseDensity, baseHeight, falloff);
        return (d0 - d1) / k;
    }
    return fogDensityAtHeight(rayOrigin.y + rayDir.y * dist * 0.5, baseDensity, baseHeight, falloff) * dist;
}

float fogAmountFromOpticalDepth(float opticalDepth)
{
    return 1.0 - exp(-opticalDepth);
}

#define FOG_MAX_VOLUMES 16
#define FOG_PATCH_CONTRAST 4.0
#define FOG_PATCH_FADE 0.7

struct FogVolume
{
    vec4 worldToLocal0;   // rows of the inverse model matrix (world -> unit box -0.5..0.5)
    vec4 worldToLocal1;
    vec4 worldToLocal2;
    vec4 colorDensity;    // xyz = color, w = density
    vec4 heightParams;    // x = base height (lowest point), y = falloff
    vec4 innerExtent;     // xyz = half extent of the dense core (soft edge)
};

layout(std140) uniform uBlockFog
{
    vec4 uFogColorDensity;   // xyz = color, w = thickness (0 = no fog)
    vec4 uFogHeightFalloff;  // x = base height, y = falloff to the top (0 = same from bottom to top), zw = not used (currently)
    vec4 uFogNoiseParams;    // x = strength of patches (0..1), y = noise frequency (1 / world size of one noise tile), z = amplitude of the wavy top (world units), w = quality (0 = wavy top only, 1 = wavy top + patches)
    vec4 uFogNoiseOffset;    // xyz = wind offset in noise texture space (wrapped to 0..1), w = slow evolution offset (wrapped to 0..1)
    vec4 uFogNoiseLod;       // x = noise texels covered by one pixel per world unit of distance (mip selection), y = noise features per world unit of ray path (averaging along long rays), zw = not used (currently)
    vec4 uFogVolumeInfo;     // x = number of active volumes
    FogVolume uFogVolumes[FOG_MAX_VOLUMES];
};

// tileable 3D noise (values 0..1, mean 0.5) on texture unit 15, bound once per frame by RendererFog
uniform sampler3D uTextureFogNoise;

// mip level for a noise lookup at a given distance from the camera (avoids shimmering in the distance)
float fogNoiseLod(float distanceToCamera)
{
    return log2(max(distanceToCamera * uFogNoiseLod.x, 1.0));
}

// additional mip level when one lookup has to stand for a whole stretch of the view ray:
// the longer the stretch, the more the noise averages out (prevents streaks in the far background)
float fogPathLod(float pathLength)
{
    return log2(max(pathLength * uFogNoiseLod.y, 1.0));
}

// noise value (0..1, mean 0.5) at a world position, drifting with the wind
float fogNoiseAt(vec3 worldPosition, float lod)
{
    vec3 uvw = worldPosition * uFogNoiseParams.y + uFogNoiseOffset.xyz + vec3(0.0, uFogNoiseOffset.w, 0.0);
    return textureLod(uTextureFogNoise, uvw, lod).r;
}

// base height of the height fog at a position on the ground plane (wavy top)
float fogBaseHeightAt(vec2 worldXZ, float lod)
{
    vec3 uvw = vec3(worldXZ.x * uFogNoiseParams.y + uFogNoiseOffset.x, uFogNoiseOffset.w, worldXZ.y * uFogNoiseParams.y + uFogNoiseOffset.z);
    float n = textureLod(uTextureFogNoise, uvw, lod).r;
    return uFogHeightFalloff.x + (n * 2.0 - 1.0) * uFogNoiseParams.z;
}

// density factor of the patches (mean 1): noise contrast boosted so that strength 1 reaches clear gaps and double density
float fogPatchFactor(float n, float lod)
{
    return 1.0 + uFogNoiseParams.x * clamp((n - 0.5) * FOG_PATCH_CONTRAST, -1.0, 1.0) * exp2(-FOG_PATCH_FADE * lod);
}

vec3 fogVolumeToLocal(int i, vec3 p)
{
    vec4 p4 = vec4(p, 1.0);
    return vec3(dot(uFogVolumes[i].worldToLocal0, p4), dot(uFogVolumes[i].worldToLocal1, p4), dot(uFogVolumes[i].worldToLocal2, p4));
}

vec3 fogVolumeDirToLocal(int i, vec3 d)
{
    return vec3(dot(uFogVolumes[i].worldToLocal0.xyz, d), dot(uFogVolumes[i].worldToLocal1.xyz, d), dot(uFogVolumes[i].worldToLocal2.xyz, d));
}

// entry and exit distance of the ray inside a local box (no hit if x >= y)
vec2 fogRayBox(vec3 localOrigin, vec3 localDir, vec3 halfExtent, float maxT)
{
    vec3 d = vec3(abs(localDir.x) < 1e-8 ? 1e-8 : localDir.x, abs(localDir.y) < 1e-8 ? 1e-8 : localDir.y, abs(localDir.z) < 1e-8 ? 1e-8 : localDir.z);
    vec3 t0 = (-halfExtent - localOrigin) / d;
    vec3 t1 = (halfExtent - localOrigin) / d;
    vec3 tMin = min(t0, t1);
    vec3 tMax = max(t0, t1);
    return vec2(max(max(max(tMin.x, tMin.y), tMin.z), 0.0), min(min(min(tMax.x, tMax.y), tMax.z), maxT));
}

// fog at a single point (reference for ray marching): xyz = density weighted color, w = density
vec4 fogDensity(vec3 worldPosition, float distanceToCamera)
{
    float lod = fogNoiseLod(distanceToCamera);
    bool patches = uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5;
    float n = (patches || uFogNoiseParams.z > 0.0) ? fogNoiseAt(worldPosition, lod) : 0.5;
    float factor = patches ? fogPatchFactor(n, lod) : 1.0;
    vec4 result = vec4(0.0);
    if (uFogColorDensity.w > 0.0)
    {
        float baseHeight = uFogHeightFalloff.x;
        if (uFogNoiseParams.z > 0.0 && uFogHeightFalloff.y > 0.0)
            baseHeight = fogBaseHeightAt(worldPosition.xz, lod);
        float d = fogDensityAtHeight(worldPosition.y, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y) * factor;
        result += vec4(uFogColorDensity.xyz * d, d);
    }
    int volumeCount = int(uFogVolumeInfo.x + 0.5);
    for (int i = 0; i < volumeCount; i++)
    {
        vec3 l = abs(fogVolumeToLocal(i, worldPosition));
        if (any(greaterThan(l, vec3(0.5))))
            continue;
        vec3 ramp = clamp((vec3(0.5) - l) / max(vec3(0.5) - uFogVolumes[i].innerExtent.xyz, vec3(0.0001)), 0.0, 1.0);
        float baseHeight = uFogVolumes[i].heightParams.x;
        if (uFogNoiseParams.z > 0.0 && uFogVolumes[i].heightParams.y > 0.0)
            baseHeight += (n * 2.0 - 1.0) * uFogNoiseParams.z;
        float d = fogDensityAtHeight(worldPosition.y, uFogVolumes[i].colorDensity.w, baseHeight, uFogVolumes[i].heightParams.y) * min(min(ramp.x, ramp.y), ramp.z) * factor;
        result += vec4(uFogVolumes[i].colorDensity.xyz * d, d);
    }
    return result;
}

// global fog along one ray: base height of the wavy top and the averaged patch factor
void fogGlobalSetup(vec3 cameraPosition, vec3 rayDir, float dist, vec3 worldPosition, out float baseHeight, out vec3 noiseFactors)
{
    baseHeight = uFogHeightFalloff.x;
    noiseFactors = vec3(1.0);
    if (uFogColorDensity.w <= 0.0)
        return;

    if (uFogNoiseParams.z > 0.0 && uFogHeightFalloff.y > 0.0)
    {
        float above = clamp((cameraPosition.y - uFogHeightFalloff.x) / uFogNoiseParams.z, 0.0, 1.0);
        bool crosses = above > 0.0 && rayDir.y < -0.0001;
        float hEnd = uFogHeightFalloff.x;
        float hCross = uFogHeightFalloff.x;
        if (!crosses || above < 1.0)
            hEnd = fogBaseHeightAt(worldPosition.xz, max(fogNoiseLod(dist), fogPathLod(dist)));
        if (crosses)
        {
            float tCross = min((uFogHeightFalloff.x - cameraPosition.y) / rayDir.y, dist);
            hCross = fogBaseHeightAt((cameraPosition + rayDir * tCross).xz, max(fogNoiseLod(tCross), fogPathLod(dist - tCross)));
        }
        baseHeight = crosses ? mix(hEnd, hCross, above) : hEnd;
    }

    if (uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5)
    {
        // one factor per third of the ray (not averaged: averaging over the whole ray flattened the patches)
        float segmentLod = fogPathLod(dist / 3.0);
        for (int i = 0; i < 3; i++)
        {
            float t = dist * (float(i) + 0.5) / 3.0;
            float lod = max(fogNoiseLod(t), segmentLod);
            noiseFactors[i] = fogPatchFactor(fogNoiseAt(cameraPosition + rayDir * t, lod), lod);
        }
    }
}

// mean soft edge factor between t0 and t1, weighted by the height falloff (heightSlope = falloff * rayDir.y)
float fogVolumeRamp(vec3 localOrigin, vec3 localDir, vec3 invWidth, float t0, float t1, float heightSlope)
{
    float r = 0.0;
    float weightSum = 0.0;
    for (int k = 0; k < 4; k++)
    {
        float t = mix(t0, t1, (float(k) + 0.5) * 0.25);
        vec3 ramp = clamp((vec3(0.5) - abs(localOrigin + localDir * t)) * invWidth, 0.0, 1.0);
        float weight = exp(clamp(-heightSlope * (t - t0), -80.0, 80.0));
        r += min(min(ramp.x, ramp.y), ramp.z) * weight;
        weightSum += weight;
    }
    return r / weightSum;
}

void fogRampPeakPair(float r, float k, vec3 f, vec3 kf, inout float best, inout float tBest)
{
    vec3 t = (f - r) / (k + kf);
    vec3 p = r + k * t;
    if (p.x < best) { best = p.x; tBest = t.x; }
    if (p.y < best) { best = p.y; tBest = t.y; }
    if (p.z < best) { best = p.z; tBest = t.z; }
}

// ray distance where the soft edge ramp peaks: lowest crossing of a rising and a falling ramp line
float fogVolumeRampPeak(vec3 localOrigin, vec3 localDir, vec3 invWidth)
{
    vec3 u = mix(-localOrigin, localOrigin, step(0.0, localDir));
    vec3 k = max(abs(localDir), vec3(1e-6)) * invWidth;
    vec3 rising = (0.5 + u) * invWidth;
    vec3 falling = (0.5 - u) * invWidth;
    float best = 1e30;
    float tBest = 0.0;
    fogRampPeakPair(rising.x, k.x, falling, k, best, tBest);
    fogRampPeakPair(rising.y, k.y, falling, k, best, tBest);
    fogRampPeakPair(rising.z, k.z, falling, k, best, tBest);
    return tBest;
}

float fogSegmentDepth(vec3 cameraPosition, vec3 rayDir, float t0, float t1, float density, float baseHeight, float falloff)
{
    return t1 > t0 ? fogOpticalDepthHeight(cameraPosition + rayDir * t0, rayDir, t1 - t0, density, baseHeight, falloff) : 0.0;
}

// global fog between t0 and t1, each third of the ray with its own patch factor
float fogGlobalDepth(vec3 cameraPosition, vec3 rayDir, float t0, float t1, float dist, float baseHeight, vec3 noiseFactors)
{
    float third = dist / 3.0;
    float tau = 0.0;
    for (int k = 0; k < 3; k++)
    {
        float a = max(t0, third * float(k));
        float b = min(t1, third * float(k + 1));
        tau += fogSegmentDepth(cameraPosition, rayDir, a, b, uFogColorDensity.w, baseHeight, uFogHeightFalloff.y) * noiseFactors[k];
    }
    return tau;
}

// optical depth of volume i along the ray: x = depth (0 = missed), yz = ray segment inside the box
vec3 fogVolumeDepth(int i, vec3 cameraPosition, vec3 rayDir, float dist)
{
    vec3 localOrigin = fogVolumeToLocal(i, cameraPosition);
    vec3 localDir = fogVolumeDirToLocal(i, rayDir);
    vec2 outer = fogRayBox(localOrigin, localDir, vec3(0.5), dist);
    if (outer.y <= outer.x)
        return vec3(0.0);
    float tMid = 0.5 * (outer.x + outer.y);

    float density = uFogVolumes[i].colorDensity.w;
    float baseHeight = uFogVolumes[i].heightParams.x;
    float falloff = uFogVolumes[i].heightParams.y;
    bool wave = uFogNoiseParams.z > 0.0 && falloff > 0.0;
    bool patches = uFogNoiseParams.x > 0.0 && uFogNoiseParams.w > 0.5;
    float factor = 1.0;
    if (wave || patches)
    {
        float noiseLod = max(fogNoiseLod(tMid), fogPathLod(outer.y - outer.x));
        float n = fogNoiseAt(cameraPosition + rayDir * tMid, noiseLod);
        if (wave)
            baseHeight += (n * 2.0 - 1.0) * uFogNoiseParams.z;
        if (patches)
            factor = fogPatchFactor(n, noiseLod);
    }

    float tau;
    vec3 shell = vec3(0.5) - uFogVolumes[i].innerExtent.xyz;
    if (max(max(shell.x, shell.y), shell.z) <= 0.0)
    {
        tau = fogSegmentDepth(cameraPosition, rayDir, outer.x, outer.y, density, baseHeight, falloff);
    }
    else
    {
        // soft edge: dense core exact, rising and falling part of the ramp sampled separately
        vec3 invWidth = 1.0 / max(shell, vec3(0.0001));
        float heightSlope = falloff * rayDir.y;
        vec2 inner = fogRayBox(localOrigin, localDir, uFogVolumes[i].innerExtent.xyz, dist);
        tau = 0.0;
        if (inner.y > inner.x)
            tau = fogSegmentDepth(cameraPosition, rayDir, inner.x, inner.y, density, baseHeight, falloff);
        else
            inner = vec2(clamp(fogVolumeRampPeak(localOrigin, localDir, invWidth), outer.x, outer.y));
        tau += fogSegmentDepth(cameraPosition, rayDir, outer.x, inner.x, density, baseHeight, falloff) * fogVolumeRamp(localOrigin, localDir, invWidth, outer.x, inner.x, heightSlope)
             + fogSegmentDepth(cameraPosition, rayDir, inner.y, outer.y, density, baseHeight, falloff) * fogVolumeRamp(localOrigin, localDir, invWidth, inner.y, outer.y, heightSlope);
    }
    return vec3(tau * factor, outer);
}

// fog along the view ray (0..dist): xyz = in-scattered fog color, w = remaining transmittance
// the global fog is taken from its own ray (globalDir, 0..globalDist with globalDist <= dist) - the view ray itself
// for all surfaces, only background pixels use a different one. Fog volumes always follow the view ray.
vec4 fogIntegrateRay(vec3 cameraPosition, vec3 rayDir, float dist, vec3 globalDir, float globalDist)
{
    int volumeCount = int(uFogVolumeInfo.x + 0.5);
    if (uFogColorDensity.w <= 0.0 && volumeCount == 0)
        return vec4(0.0, 0.0, 0.0, 1.0);

    float baseHeight;
    vec3 noiseFactors;
    fogGlobalSetup(cameraPosition, globalDir, globalDist, cameraPosition + globalDir * globalDist, baseHeight, noiseFactors);

    // volumes hit by the ray (optical depth spread evenly over the segment)
    vec2 hitSegment[FOG_MAX_VOLUMES];
    float hitExtinction[FOG_MAX_VOLUMES];
    int hitIndex[FOG_MAX_VOLUMES];
    int hits = 0;
    for (int i = 0; i < volumeCount; i++)
    {
        vec3 v = fogVolumeDepth(i, cameraPosition, rayDir, dist);
        if (v.x <= 0.0)
            continue;
        hitSegment[hits] = v.yz;
        hitExtinction[hits] = v.x / max(v.z - v.y, 0.000001);
        hitIndex[hits] = i;
        hits++;
    }

    // walk from segment border to segment border, overlapping media mixed per interval
    vec3 inscatter = vec3(0.0);
    float transmittance = 1.0;
    float t = 0.0;
    for (int s = 0; s <= 2 * FOG_MAX_VOLUMES; s++)
    {
        float tNext = dist;
        for (int k = 0; k < hits; k++)
        {
            if (hitSegment[k].x > t)
                tNext = min(tNext, hitSegment[k].x);
            if (hitSegment[k].y > t)
                tNext = min(tNext, hitSegment[k].y);
        }

        vec4 medium = vec4(0.0); // xyz = color weighted by optical depth, w = optical depth
        if (uFogColorDensity.w > 0.0)
        {
            float tauGlobal = fogGlobalDepth(cameraPosition, globalDir, t, tNext, globalDist, baseHeight, noiseFactors);
            medium += vec4(uFogColorDensity.xyz * tauGlobal, tauGlobal);
        }
        float tCenter = 0.5 * (t + tNext);
        for (int k = 0; k < hits; k++)
        {
            if (tCenter > hitSegment[k].x && tCenter < hitSegment[k].y)
            {
                float tau = hitExtinction[k] * (tNext - t);
                medium += vec4(uFogVolumes[hitIndex[k]].colorDensity.xyz * tau, tau);
            }
        }
        if (medium.w > 0.0)
        {
            float tr = exp(-medium.w);
            inscatter += transmittance * (1.0 - tr) * medium.xyz / medium.w;
            transmittance *= tr;
        }

        t = tNext;
        if (t >= dist)
            break;
    }
    return vec4(inscatter, transmittance);
}

// fog between camera and world position: xyz = in-scattered fog color, w = remaining transmittance
vec4 fogIntegrate(vec3 cameraPosition, vec3 worldPosition)
{
    vec3 cameraToFragment = worldPosition - cameraPosition;
    float dist = length(cameraToFragment);
    vec3 rayDir = cameraToFragment / max(dist, 0.0001);
    return fogIntegrateRay(cameraPosition, rayDir, dist, rayDir, dist);
}

// fog in front of the background (nothing rendered there, farPosition lies on the far plane)
vec4 fogIntegrateBackground(vec3 cameraPosition, vec3 farPosition)
{
    vec3 cameraToFar = farPosition - cameraPosition;
    float dist = length(cameraToFar);
    vec3 rayDir = cameraToFar / max(dist, 0.0001);
    vec3 globalDir = rayDir;
    float globalDist = dist;
    if (rayDir.y < 0.0 && uFogColorDensity.w > 0.0)
    {
        // Below the horizon a ray to the far plane runs into denser and denser height fog, so the distant fog
        // would look like a flat disc with a hard edge at the horizon. Instead the background gets the stronger of:
        // a) a ground that continues the scene at the base height (at least a little below the camera)
        // b) the fog of the sky at the same angle above the horizon
        // Both match the sky at the horizon, so there is no edge.
        float falloff = uFogHeightFalloff.y;
        float groundHeight = min(uFogHeightFalloff.x, cameraPosition.y - (falloff > 1.0 ? 1.0 / falloff : 1.0));
        float groundDist = min((cameraPosition.y - groundHeight) / -rayDir.y, dist);
        vec3 skyDir = vec3(rayDir.x, -rayDir.y, rayDir.z);
        float tauGround = fogOpticalDepthHeight(cameraPosition, rayDir, groundDist, uFogColorDensity.w, uFogHeightFalloff.x, falloff);
        float tauSky = fogOpticalDepthHeight(cameraPosition, skyDir, dist, uFogColorDensity.w, uFogHeightFalloff.x, falloff);
        if (tauSky > tauGround)
            globalDir = skyDir;
        else
            globalDist = groundDist;
    }
    return fogIntegrateRay(cameraPosition, rayDir, dist, globalDir, globalDist);
}

// return value: 0 = clear sight, 1 = completely fogged
float fogAmount(vec3 cameraPosition, vec3 worldPosition)
{
    return 1.0 - fogIntegrate(cameraPosition, worldPosition).w;
}

// mixes fog into a lit surface color (call before splitting into color and bloom)
vec3 applyFog(vec3 color, vec3 worldPosition, vec3 cameraPosition)
{
    vec4 fog = fogIntegrate(cameraPosition, worldPosition);
    return color * fog.w + fog.xyz;
}

void main()
{
    vec3 viewDir = normalize(vTangentView - vTangentPosition);
	vTexture2 = ParallaxMapping(vTexture, viewDir);

    vec4 albedo = getAlbedo();
    if(albedo.w <= 0) discard;

    vec4 normalId = getNormalId();

    // actual shading:
    vec4 pbr = getPBR();
    vec4 fragPositionDepth = getFragmentPositionAndDepth();
    vec3 emissive = getEmissive();

    vec3 N = normalId.xyz;
    vec3 V = normalize(uCameraPosition.xyz - fragPositionDepth.xyz);
    vec3 F0 = getF0(uMetallicType);
    F0 = mix(F0, albedo.xyz, pbr.y);

    vec3 colorTemp = vec3(0.0);
    if(abs(uShadowCaster) > 1)
    {
        colorTemp = albedo.xyz + emissive;
        color = vec4(colorTemp, albedo.w);
    }
    else
    {
        vec3 Lo = vec3(0.0);
        for(int i = 0; i < uLightCount * 17; i += 17)
        {
            vec3 currentLightPos = vec3(uLights[i + 0], uLights[i + 1], uLights[i + 2]);
            vec3 currentLightLAV = vec3(uLights[i + 4], uLights[i + 5], uLights[i + 6]);
            vec4 currentLightClr = vec4(uLights[i + 7], uLights[i + 8], uLights[i + 9], uLights[i + 10]);
            float currentLightNear = uLights[i + 11];
            float currentLightFar = uLights[i + 12];
            float currentLightFOV = uLights[i + 13];
            int currentLightType = int(uLights[i + 14]); //  0 = point,  -1 = sun,       1 = directional
            int shadowMapIndex = int(uLights[i + 3]); // -1 = cubemap, 0 = no shadow, 1 = texture2D
            float currentLightBias = uLights[i + 15];
            float currentLightHardness = uLights[i + 16];

            // calculate per-light radiance
            if(currentLightType < 0)
            {
                currentLightPos = fragPositionDepth.xyz - currentLightLAV;
            }
            else if(currentLightType == 0)
            {
               if(length(currentLightPos - vPosition.xyz) > currentLightFar)
               {
                    continue;
               }
            }
            else // directional
            {
                vec3 lightMiddlePos = currentLightPos + currentLightLAV * currentLightFar * 0.5;
                if(length(lightMiddlePos - vPosition.xyz) > currentLightFar * 0.5)
                {
                    continue;
                }
            }

            vec3 L = normalize(currentLightPos - fragPositionDepth.xyz);
            vec3 H = normalize(V + L);
            float dist    = length(currentLightPos - fragPositionDepth.xyz);

            float differenceLightDirectionAndFragmentDirection = 1.0;
		    if(currentLightType > 0)
		    {
			    float theta     = dot(L, -currentLightLAV);
                float spotInnerCutOff = 0.99;
                float spotOuterCutOff = 1.0 - (currentLightFOV * currentLightFOV) / (179.0 * 179.0 * 4.0);
			    float epsilon   = max(spotInnerCutOff - spotOuterCutOff, 0.000001);
			    differenceLightDirectionAndFragmentDirection = clamp((theta - spotOuterCutOff) / epsilon, 0.0, 1.0);    
		    }

            //float attenuation = currentLightFar / (dist * dist);
            float theDistanceClamped = clamp(dist, 0.0, currentLightFar);
            float attenuation = currentLightClr.w * (currentLightType < 0 ? 1.0 : cos(ninetydegrees / currentLightFar * theDistanceClamped));
            vec3 radiance     = currentLightClr.xyz * attenuation * differenceLightDirectionAndFragmentDirection; 


            // cook-torrance brdf
            float NDF = DistributionGGX(N, H, pbr.z); //z = roughness
            float G   = GeometrySmith(N, V, L, pbr.z);      
            vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);

            vec3 kS = F;
            vec3 kD = vec3(1.0) - kS;
            kD *= 1.0 - pbr.y;	 

            vec3 numerator    = NDF * G * F;
            float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
            vec3 specular     = numerator / denominator;  

            // add to outgoing radiance Lo
            float NdotL = max(dot(N, L), 0.0);                

            // shadow map check:
            float darkeningCurrentLight = 1.0;
            if(uShadowCaster > 0)
            {
                if(shadowMapIndex > 0) // directional or sun light
                {
                    vec3 projCoordsForTextureLookup = (vShadowCoord[shadowMapIndex - 1].xyz / vShadowCoord[shadowMapIndex - 1].w) * 0.5 + 0.5;
                    vec3 projCoordsForTextureLookup2 = (vShadowCoordOuter[shadowMapIndex - 1].xyz / vShadowCoordOuter[shadowMapIndex - 1].w) * 0.5 + 0.5;
                    vec2 shadowVisibility = getShadowMapVisiblity(projCoordsForTextureLookup, projCoordsForTextureLookup2);
                    if(uUseTextureReflectionQuality.w > 0)
                    {
                        darkeningCurrentLight = 0;
                        float offset = 1.0 / textureSize(uShadowMap[shadowMapIndex - 1], 0).x;
                        for(int i = 0; i < 5; i++)
                        {
                            vec2 currentOffset = vec2(offset * fragOffsets[i].x, offset * fragOffsets[i].y);
			                vec4 b = texture(uShadowMap[shadowMapIndex - 1], vec3((shadowVisibility.y > 0 ? projCoordsForTextureLookup2.xy : projCoordsForTextureLookup.xy) + currentOffset, shadowVisibility.y));
                            float fragmentDepthLinearized = shadowVisibility.y > 0 ? projCoordsForTextureLookup2.z : projCoordsForTextureLookup.z;
                            if(currentLightType == 1)
                            {
                                fragmentDepthLinearized = ((shadowVisibility.y > 0 ? vShadowCoordOuter[shadowMapIndex - 1].z : vShadowCoord[shadowMapIndex - 1].z) - currentLightNear) / (currentLightFar - currentLightNear);
                            }
                            darkeningCurrentLight += calculateShadow(b, fragmentDepthLinearized, currentLightBias * fragOffsetsWeights2[i], currentLightHardness) * fragOffsetsWeights[i];
                        }
                    }
                    else{
			            vec4 b = texture(uShadowMap[shadowMapIndex - 1], vec3((shadowVisibility.y > 0 ? projCoordsForTextureLookup2.xy : projCoordsForTextureLookup.xy), shadowVisibility.y));
                        float fragmentDepthLinearized = shadowVisibility.y > 0 ? projCoordsForTextureLookup2.z : projCoordsForTextureLookup.z;
                        if(currentLightType == 1)
                        {
                            fragmentDepthLinearized = ((shadowVisibility.y > 0 ? vShadowCoordOuter[shadowMapIndex - 1].z : vShadowCoord[shadowMapIndex - 1].z) - currentLightNear) / (currentLightFar - currentLightNear);
                        }
			            darkeningCurrentLight = calculateShadow(b, fragmentDepthLinearized, currentLightBias, currentLightHardness);
                    }
                    darkeningCurrentLight = mix(darkeningCurrentLight, 1.0, shadowVisibility.x);
                }
                else if(shadowMapIndex < 0) // point light
                {
                    darkeningCurrentLight = calculateShadowCube(
                        abs(shadowMapIndex) - 1, 
                        currentLightPos.xyz, 
                        fragPositionDepth.xyz, 
                        vec2(currentLightNear, currentLightFar), 
                        currentLightBias, 
                        currentLightHardness);
                }
            }

            Lo += (kD * albedo.xyz / PI + specular) * radiance * NdotL * darkeningCurrentLight;
        }
        vec3 reflectionColor = getReflectionColor(V, N, pbr.z, fragPositionDepth.xyz);// z = roughness
        vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, pbr.z); // z = roughness
        vec3 kDW = 1.0 - F;
        kDW *= (1.0 - pbr.y); // y = metallic	
        vec3 specularW = reflectionColor * F * uColorAmbient; 
        vec3 ambient = uColorAmbient * kDW * albedo.xyz + specularW;
        colorTemp = ambient + Lo;
        color = vec4(colorTemp + emissive, albedo.w);
    }

    // fog:
    color.xyz = applyFog(color.xyz, fragPositionDepth.xyz, uCameraPosition.xyz);

    float bloomR = max(0.0, color.x - 1.0);
    float bloomG = max(0.0, color.y - 1.0);
    float bloomB = max(0.0, color.z - 1.0);
    color.x -= bloomR;
    color.y -= bloomG;
    color.z -= bloomB;
    bloom = vec4(bloomR, bloomG, bloomB, albedo.w);
}

