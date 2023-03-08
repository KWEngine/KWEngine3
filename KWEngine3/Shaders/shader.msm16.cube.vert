#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 5) in	ivec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

uniform int uUseAnimations;
uniform mat4 uModelMatrix;
uniform mat4 uBoneTransforms[128];
uniform vec3 uTextureTransformOpacity;

out vec2 vTexture;

void main()
{
	mat4 BoneTransform = mat4(0.0);
	if(uUseAnimations > 0)
	{	
		BoneTransform += uBoneTransforms[aBoneIds[0]] * aBoneWeights[0];
		BoneTransform += uBoneTransforms[aBoneIds[1]] * aBoneWeights[1];
		BoneTransform += uBoneTransforms[aBoneIds[2]] * aBoneWeights[2];
	}
	else
	{
		BoneTransform = mat4(1.0);
	}
	vec4 totalLocalPos = BoneTransform * vec4(aPosition, 1.0);

	vTexture = aTexture * uTextureTransformOpacity.xy;
	gl_Position = uModelMatrix * totalLocalPos;
}