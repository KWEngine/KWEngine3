#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;
layout(location = 5) in	uvec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec4 uTextureTransform;
uniform vec2 uTextureClip;
uniform int uUseAnimations;
uniform mat4 uBoneTransforms[128];
uniform mat4 uViewProjectionMatrixShadowMap[3];
uniform mat4 uViewProjectionMatrixShadowMapOuter[3];
uniform vec4 uCameraPosition;

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;
out mat3 vTBN;
out vec4 vShadowCoord[3];
out vec4 vShadowCoordOuter[3];
out vec3 vTangentView;
out vec3 vTangentPosition;

void main()
{
	vec4 totalLocalPos = vec4(0.0);
	vec4 totalNormal = vec4(0.0);
	vec4 totalTangent = vec4(0.0);
	vec4 totalBiTangent = vec4(0.0);
	
	if(uUseAnimations > 0)
	{	
		for(int i = 0; i < 3; i++)
		{
			totalLocalPos += aBoneWeights[i] * uBoneTransforms[aBoneIds[i]] * vec4(aPosition, 1.0);
			totalNormal  += aBoneWeights[i] * uBoneTransforms[aBoneIds[i]] * vec4(aNormal, 0.0);
			totalTangent += aBoneWeights[i] * uBoneTransforms[aBoneIds[i]] * vec4(aTangent, 0.0);
			totalBiTangent  += aBoneWeights[i] * uBoneTransforms[aBoneIds[i]] * vec4(aBiTangent, 0.0);
		}
	}
	else
	{
		totalLocalPos = vec4(aPosition, 1.0);
		totalNormal = vec4(aNormal, 0.0);
		totalTangent = vec4(aTangent, 0.0);
		totalBiTangent = vec4(aBiTangent, 0.0);
	}

	gl_Position = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vPosition = uModelMatrix * totalLocalPos;
	for(int i = 0; i < 3; i++)
	{
		vShadowCoord[i] = uViewProjectionMatrixShadowMap[i] * uModelMatrix * totalLocalPos;
		vShadowCoordOuter[i] = uViewProjectionMatrixShadowMapOuter[i] * uModelMatrix * totalLocalPos;
	}
	vNormal = normalize((uNormalMatrix * totalNormal).xyz);
	vTangent = normalize((uNormalMatrix * totalTangent).xyz);
	vBiTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTBN = mat3(vTangent.xyz, -vBiTangent.xyz, vNormal.xyz);
	
	vTexture = (vec2(uTextureTransform.x < 0 ? 1.0 - aTexture.x : aTexture.x, uTextureTransform.y < 0 ? 1.0 - aTexture.y : aTexture.y) + uTextureTransform.zw) * abs(uTextureTransform.xy);
	vec2 uvCenter = uTextureTransform.zw * abs(uTextureTransform.xy) + abs(uTextureTransform.xy) * 0.5;
	vec2 delta = vTexture - uvCenter;
	vTexture = vTexture + delta * uTextureClip;

	mat3 vTBN2 = transpose(mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz));
	vTangentPosition = vTBN2 * vPosition.xyz;
	vTangentView = vTBN2 * uCameraPosition.xyz;
}