#version 400 core

in vec2 vTexture;
//in vec2 vPositionNDC;
flat in int vInstanceID;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTexturePBR; //x=metallic, y = roughness, z = metallic type
uniform sampler2D uTextureDepth;
uniform sampler2D uTextureId;
uniform sampler2D uTextureSSAO;

uniform sampler2DArray uShadowMap[3];
uniform samplerCubeArray uShadowMapCube[3];

uniform samplerCube uTextureSkybox;
uniform mat3 uTextureSkyboxRotation;
uniform sampler2D uTextureBackground;
uniform ivec4 uUseTextureReflectionQuality;
uniform mat4 uViewProjectionMatrixShadowMap[3];
uniform mat4 uViewProjectionMatrixShadowMap2[3];

uniform float uLights[850];

// multi-draw tiling:
//uniform int uLightIndices[50];
//uniform int uLightIndicesCount;

uniform vec3 uCameraPos;
uniform vec3 uColorAmbient;
uniform mat4 uViewProjectionMatrixInverted;

// instanced tiled drawing:
uniform int uLightIndicesCounts[50];
layout (std140) uniform uBlockIndex3
{
    ivec4 instanceData3[(50 * 64) / 4];
};


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

vec3 decodeNormalFromRG16F(vec2 enc) {
    vec2 f = enc;

    vec3 n = vec3(f.xy, 1.0 - abs(f.x) - abs(f.y));

    if (n.z < 0.0) {
        n.xy = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
    }

    return normalize(n);
}

vec3 decodeNormal(vec2 f)
{
    f = f * 2.0 - 1.0;
    vec3 n = vec3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));
    float t = clamp(-n.z, 0.0, 1.0);
    n.x += n.x >= 0.0 ? -t : t;
    n.y += n.y >= 0.0 ? -t : t;
    return normalize(n);
}

vec3 sampleFromEquirectangular(vec3 worldPosition, vec3 normal, float mipMapLevel)
{
    vec3 R = normalize(reflect(worldPosition - uCameraPos, normal));
    vec2 uv;
    uv.x = atan( -R.z, -R.x ) * PI2 + 0.5;
    uv.y = 1.0 - (R.y * 0.5 + 0.5);

    return textureLod(uTextureBackground, uv, mipMapLevel).xyz;
}

vec3 getF0(int type)
{
    return metallicF0Values[type];
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

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness) //, float metallic)
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

/*
ivec2 getIdShadowCaster()
{
    vec3 s = texture(uTextureId, vTexture).xyz;
    uint id = decode8BitTo16BitUint(s.xy);
    int z = int(round(s.z * 255.0)) - 10;
    return ivec2(id, z);  
}
*/

uint decode8BitTo16BitUint(vec2 encoded)
{
    uint low8 = uint(round(encoded.y * 255.0));
    uint high8 = uint(round(encoded.x * 255.0));
    return high8 * 256 + low8;
}

uint getId(vec3 s)
{
    return decode8BitTo16BitUint(s.xy);
}

int getShadowCaster(vec3 s)
{
    return int(round(s.z * 255.0)) - 10;
}

vec3 getPBR()
{
    vec3 tmp = texture(uTexturePBR, vTexture).xyz; //x=metallic, y = roughness, z = metallic type
    tmp.z = round(tmp.z * 9);
    return tmp;
}

vec3 getAlbedo()
{
    return texture(uTextureAlbedo, vTexture).xyz;
}

vec3 getNormal()
{
    return decodeNormalFromRG16F(texture(uTextureNormal, vTexture).xy);
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

vec4 getFragmentPosition()
{
    float depth01 = texture(uTextureDepth, vTexture).r;
    float depth = depth01 * 2.0 - 1.0;
    vec4 clipSpaceCoordinate = vec4(vTexture * 2.0 - 1.0, depth, 1.0);
    vec4 worldSpaceCoordinate = uViewProjectionMatrixInverted * clipSpaceCoordinate;
    worldSpaceCoordinate.xyz /= worldSpaceCoordinate.w;
    return vec4(worldSpaceCoordinate.xyz, depth01);
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

void main()
{
    vec3 sampleIdShadowCaster = texture(uTextureId, vTexture).xyz;
    uint id  = getId(sampleIdShadowCaster);

    if(id == 0) { discard; }

    // actual shading:
    vec3 albedo = getAlbedo();
    int shadowCaster = getShadowCaster(sampleIdShadowCaster);
    vec3 normal = getNormal();
    vec3 pbr = getPBR();
	
    vec3 emissive = vec3(max(0, albedo.x - 1.0), max(0, albedo.y - 1.0), max(0, albedo.z - 1.0));
    albedo = vec3(min(albedo.x, 1.0), min(albedo.y, 1.0), min(albedo.z, 1.0));
    
    vec4 fragPosition4 = getFragmentPosition();
    vec3 fragPosition = fragPosition4.xyz;
    vec3 N = normal;
    vec3 V = normalize(uCameraPos - fragPosition);
    vec3 F0 = getF0(int(pbr.z));
    F0 = mix(F0, albedo, pbr.x);

    int lightIndicesCount = uLightIndicesCounts[vInstanceID];

    vec3 colorTemp = vec3(0.0);
    if(abs(shadowCaster) > 1)
    {
        colorTemp = albedo + emissive;
        color = vec4(colorTemp, 1.0);
    }
    else
    {
        vec3 Lo = vec3(0.0);
        
        //for(int li = 0; li < uLightIndicesCount; li++) // multi-draw
        for(int li = 0; li < lightIndicesCount; li++)
        {
            // instancing mode:
            int index = vInstanceID * 50 + li;
            int moduloLight = index % 4;
            int divisoLight = index / 4;
            int i = instanceData3[divisoLight][moduloLight] * 19;
            

            // multi-draw mode:
            //int i = uLightIndices[li] * 17;

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
                currentLightPos = fragPosition - currentLightLAV;
            }
            vec3 L = normalize(currentLightPos - fragPosition);
            vec3 H = normalize(V + L);
            float dist    = length(currentLightPos - fragPosition);

            float differenceLightDirectionAndFragmentDirection = 1.0;
		    if(currentLightType > 0)
		    {
			    float theta     = dot(L, -currentLightLAV);
                float spotInnerCutOff = 0.99;
                float spotOuterCutOff = 1.0 - (currentLightFOV * currentLightFOV) / (179.0 * 179.0 * 4.0);
			    float epsilon   = max(spotInnerCutOff - spotOuterCutOff, 0.000001);
			    differenceLightDirectionAndFragmentDirection = clamp((theta - spotOuterCutOff) / epsilon, 0.0, 1.0);    
		    }

            float theDistanceClamped = clamp(dist, 0.0, currentLightFar);
            float attenuation = currentLightClr.w * (currentLightType < 0 ? 1.0 : cos(ninetydegrees / currentLightFar * theDistanceClamped));
            vec3 radiance     = currentLightClr.xyz * attenuation * differenceLightDirectionAndFragmentDirection; 


            // cook-torrance brdf
            float NDF = DistributionGGX(N, H, pbr.y); //y = roughness
            float G   = GeometrySmith(N, V, L, pbr.y);      
            vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);

        
            vec3 kS = F;
            vec3 kD = vec3(1.0) - kS;
            kD *= 1.0 - pbr.x;	 // x = metallic

            vec3 numerator    = NDF * G * F;
            float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
            vec3 specular     = numerator / denominator;  

            // add to outgoing radiance Lo
            float NdotL = max(dot(N, L), 0.0);                

            // shadow map check:
            float darkeningCurrentLight = 1.0;
            if(shadowCaster > 0)
            {
                
                if(shadowMapIndex > 0) // directional or sun light
                {
                    vec4 vShadowCoord = uViewProjectionMatrixShadowMap[shadowMapIndex - 1] * vec4(fragPosition, 1.0);
                    vec4 vShadowCoordOuter = uViewProjectionMatrixShadowMap2[shadowMapIndex - 1] * vec4(fragPosition, 1.0);
                    
                    // if the light is directional, we first have to linearize the depth values:
			        vec3 projCoordsForTextureLookup = (vShadowCoord.xyz / vShadowCoord.w) * 0.5 + 0.5;
                    vec3 projCoordsForTextureLookup2 = (vShadowCoordOuter.xyz / vShadowCoordOuter.w) * 0.5 + 0.5;
                    
                    vec2 shadowVisibility = getShadowMapVisiblity(projCoordsForTextureLookup, projCoordsForTextureLookup2);

                    if(uUseTextureReflectionQuality.w > 0)
                    {
                        darkeningCurrentLight = 0;
                        float offset = 1.0 / textureSize(uShadowMap[shadowMapIndex - 1], 0).x;
                        for(int i = 0; i < 5; i++)
                        {
                            vec2 currentOffset = vec2(offset * fragOffsets[i].x, offset * fragOffsets[i].y);
			                vec4 b = texture(uShadowMap[shadowMapIndex - 1], vec3(shadowVisibility.y > 0 ? projCoordsForTextureLookup2.xy : projCoordsForTextureLookup.xy + currentOffset, shadowVisibility.y));
                            float fragmentDepthLinearized = shadowVisibility.y > 0 ? projCoordsForTextureLookup2.z : projCoordsForTextureLookup.z;
                            if(currentLightType == 1)
                            {
                                fragmentDepthLinearized = ((shadowVisibility.y > 0 ? vShadowCoordOuter.z : vShadowCoord.z) - currentLightNear) / (currentLightFar - currentLightNear);
                            }
                            darkeningCurrentLight += calculateShadow(b, fragmentDepthLinearized, currentLightBias * fragOffsetsWeights2[i], currentLightHardness) * fragOffsetsWeights[i];
                        }
                    }
                    else
                    {
                            vec4 b = texture(uShadowMap[shadowMapIndex - 1], vec3(shadowVisibility.y > 0 ? projCoordsForTextureLookup2.xy : projCoordsForTextureLookup.xy, shadowVisibility.y));
                            float fragmentDepthLinearized = shadowVisibility.y > 0 ? projCoordsForTextureLookup2.z : projCoordsForTextureLookup.z;
                            if(currentLightType == 1)
                            {
                                fragmentDepthLinearized = ((shadowVisibility.y > 0 ? vShadowCoordOuter.z : vShadowCoord.z) - currentLightNear) / (currentLightFar - currentLightNear);
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
                        fragPosition, 
                        vec2(currentLightNear, currentLightFar), 
                        currentLightBias, 
                        currentLightHardness);
                }
            }

            Lo += (kD * albedo / PI + specular) * radiance * NdotL * darkeningCurrentLight;
        }
        vec3 reflectionColor = getReflectionColor(V, N, pbr.y, fragPosition);// y = roughness
        vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, pbr.y); // y = roughness
        vec3 kDW = 1.0 - F;
        kDW *= (1.0 - pbr.x); // x = metallic	
        vec3 specularW = reflectionColor * F * uColorAmbient; 
        vec3 ambient = uColorAmbient * texture(uTextureSSAO, vTexture).r * kDW * albedo + specularW + emissive;
        colorTemp = ambient + Lo;
        color = vec4(colorTemp, 1.0);
    }
/*    
    if((vPositionNDC.x > 0.99 || vPositionNDC.x < -0.99) || (vPositionNDC.y > 0.99 || vPositionNDC.y < -0.99))
    {
        if(lightIndicesCount == 0)
            color = vec4(0.00, 0.00, 0.00, 1);
        else if(lightIndicesCount == 1)
            color = vec4(0.00, 1.00, 0.00, 1);
        else if(lightIndicesCount == 2)
            color = vec4(0.25, 1.00, 0.00, 1);
        else if(lightIndicesCount == 2)
            color = vec4(0.50, 1.00, 0.00, 1);
        else if(lightIndicesCount == 3)
            color = vec4(0.75, 1.00, 0.00, 1);
        else if(lightIndicesCount == 4)
            color = vec4(1.00, 1.00, 0.00, 1);
        else if(lightIndicesCount == 5)
            color = vec4(1.00, 0.75, 0.00, 1);
        else if(lightIndicesCount == 6)
            color = vec4(1.00, 0.50, 0.00, 1);
        else if(lightIndicesCount == 7)
            color = vec4(1.00, 0.25, 0.00, 1);
        else if(lightIndicesCount == 8)
            color = vec4(1.00, 0.00, 0.00, 1);
        else if(lightIndicesCount == 9)
            color = vec4(1.00, 0.00, 0.25, 1);
        else if(lightIndicesCount == 10)
            color = vec4(1.00, 0.00, 0.50, 1);
        else if(lightIndicesCount == 11)
            color = vec4(1.00, 0.00, 0.75, 1);
        else
            color = vec4(1.00, 0.00, 1.00, 1);
    }
*/
    
    float bloomR = 0.0;
    float bloomG = 0.0;
    float bloomB = 0.0;
    if(colorTemp.x > 1.0)
        bloomR = colorTemp.x - 1.0;
    if(colorTemp.y > 1.0)
        bloomG = colorTemp.y - 1.0;
    if(colorTemp.z > 1.0)
        bloomB = colorTemp.z - 1.0;
    bloom = vec4(bloomR, bloomG, bloomB, 1.0);

    gl_FragDepth = id >= 32768 && id < 65535 ? 0.0 : fragPosition4.w;
}

