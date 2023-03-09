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
uniform int uUseAnimations;
uniform mat4 uBoneTransforms[128];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;
out mat3 vTBN;

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
	vNormal = normalize((uNormalMatrix * totalNormal).xyz);
	vTangent = normalize((uNormalMatrix * totalTangent).xyz);
	vBiTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTexture = (aTexture  + uTextureTransform.zw) * uTextureTransform.xy;
	vTBN = mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz);
}