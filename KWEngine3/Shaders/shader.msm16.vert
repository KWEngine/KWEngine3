#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 5) in	uvec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

out		float vZ;
out		float vW;
out		vec2  vTexture;

uniform int uUseAnimations;
uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uBoneTransforms[128];
uniform vec3 uTextureTransformOpacity;
uniform vec2 uTextureOffset;
uniform vec2 uTextureClip;


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
	vec4 transformedPosition = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vZ = transformedPosition.z;
	vW = transformedPosition.w;

	vTexture = vec2(uTextureTransformOpacity.x < 0.0 ? 1.0 - aTexture.x : aTexture.x, uTextureTransformOpacity.y < 0.0 ? 1.0 - aTexture.y : aTexture.y) * abs(uTextureTransformOpacity.xy) + uTextureOffset;
	vec2 uvCenter = uTextureOffset * abs(uTextureTransformOpacity.xy) + abs(uTextureTransformOpacity.xy) * 0.5;
	vec2 delta = vTexture - uvCenter;
	vTexture = vTexture + delta * uTextureClip;

	gl_Position = transformedPosition;
}