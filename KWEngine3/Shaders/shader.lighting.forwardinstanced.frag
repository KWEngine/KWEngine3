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
    if(bQuantized == vec4(1.0, 1.0, 1.0, 1.0))
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

float calculateShadowCube(int index, vec3 lightPos, vec3 fragPos, vec2 lightNearFar, float bias, float hardness)
{
	vec3 lightToFrag = fragPos - lightPos;
	float currentDepth = length(lightToFrag);
	float fragmentDepthLinearized = (currentDepth - lightNearFar.x) / (lightNearFar.y - lightNearFar.x);
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
        albedo = vec4(hueShift(tmp.xyz, uMetallicRoughness.z), tmp.w) * uColorTint;
    }
    else
    {
        
        albedo = vec4(hueShift(uColorMaterial.xyz, uMetallicRoughness.z), uColorMaterial.w) * uColorTint;
    }

    vec4 alpha = getAlphaTexture();
    albedo.w = min(albedo.w, alpha.w);

    return albedo;
}

vec4 getEmissive()
{
    if(uUseTexturesAlbedoNormalEmissive.z > 0)
    {
        vec4 emissiveFromTexture = texture(uTextureEmissive, vTexture2);
        vec4 result = vec4(hueShift(emissiveFromTexture.xyz * emissiveFromTexture.w, uMetallicRoughness.z), uColorEmissive.w);
        return result;
    }
    else
    {
        return uColorEmissive;
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

	const float minLayers = 8.0;
	const float maxLayers = 32.0;
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
	vTexture2 = ParallaxMapping(vTexture, viewDir);

    vec4 normalId = getNormalId();

    // actual shading:
    vec4 pbr = getPBR();
    vec4 fragPositionDepth = getFragmentPositionAndDepth();
	vec4 albedo = getAlbedo();
    vec4 emissive4 = getEmissive();
    vec3 emissive = emissive4.xyz;

    vec3 N = normalId.xyz;
    vec3 V = normalize(uCameraPosition.xyz - fragPositionDepth.xyz);
    vec3 F0 = getF0(uMetallicType);
    F0 = mix(F0, albedo.xyz, pbr.y);

    vec3 colorTemp = vec3(0.0);
    if(abs(uShadowCaster) > 1)
    {
        colorTemp = albedo.xyz * albedo.w + emissive * emissive4.w;
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
            //Lo += (kD + specular) * radiance * NdotL * darkeningCurrentLight;
        }
        vec3 reflectionColor = getReflectionColor(V, N, pbr.z, fragPositionDepth.xyz);// z = roughness
        vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, pbr.z); // z = roughness
        vec3 kDW = 1.0 - F;
        kDW *= (1.0 - pbr.y); // y = metallic	
        vec3 specularW = reflectionColor * F * uColorAmbient; 
        vec3 ambient = uColorAmbient * kDW * albedo.xyz + specularW + emissive * emissive4.w;
        colorTemp = ambient + Lo;
        color = vec4(colorTemp, albedo.w);
    }

    float bloomR = 0.0;
    float bloomG = 0.0;
    float bloomB = 0.0;
    if(colorTemp.x > 1.0)
        bloomR = colorTemp.x - 1.0;
    if(colorTemp.y > 1.0)
        bloomG = colorTemp.y - 1.0;
    if(colorTemp.z > 1.0)
        bloomB = colorTemp.z - 1.0;
    bloom = vec4(bloomR, bloomG, bloomB, albedo.w);
}

