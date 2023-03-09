#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 5) in	vec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

out		float vZ;
out		float vW;
out		vec2  vTexture;

uniform int uUseAnimations;
uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uBoneTransforms[128];
uniform vec3 uTextureTransformOpacity;


void main()
{
	mat4 BoneTransform = mat4(0.0);
	if(uUseAnimations > 0)
	{	
		BoneTransform += uBoneTransforms[int(aBoneIds[0])] * aBoneWeights[0];
		BoneTransform += uBoneTransforms[int(aBoneIds[1])] * aBoneWeights[1];
		BoneTransform += uBoneTransforms[int(aBoneIds[2])] * aBoneWeights[2];
	}
	else
	{
		BoneTransform = mat4(1.0);
	}
	vec4 totalLocalPos = BoneTransform * vec4(aPosition, 1.0);
	vec4 transformedPosition = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vZ = transformedPosition.z;
	vW = transformedPosition.w;
	vTexture = aTexture * uTextureTransformOpacity.xy;
	gl_Position = transformedPosition;
}