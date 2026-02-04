#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 5) in	uvec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

out		vec2  vTexture;

uniform int uUseAnimations;
uniform mat4 uModelMatrix;
uniform mat4 uBoneTransforms[128];
uniform vec3 uTextureTransformOpacity;
uniform vec2 uTextureOffset;
uniform vec2 uTextureClip;

layout (std140) uniform uInstanceBlock
{
									// base alignment	// aligned offset
	mat4 instanceModelMatrix[1024];	// 16				// 0
};

void main()
{
	vec4 totalLocalPos = vec4(0.0);
	if(uUseAnimations > 0)
	{	
		totalLocalPos += aBoneWeights[0] * uBoneTransforms[aBoneIds[0]] * vec4(aPosition, 1.0);
		totalLocalPos += aBoneWeights[1] * uBoneTransforms[aBoneIds[1]] * vec4(aPosition, 1.0);
		totalLocalPos += aBoneWeights[2] * uBoneTransforms[aBoneIds[2]] * vec4(aPosition, 1.0);
	}
	else
	{
		totalLocalPos = vec4(aPosition, 1.0);
	}
	//totalLocalPos.w = 1.0;

	vTexture = vec2(uTextureTransformOpacity.x < 0.0 ? 1.0 - aTexture.x : aTexture.x, uTextureTransformOpacity.y < 0.0 ? 1.0 - aTexture.y : aTexture.y) * abs(uTextureTransformOpacity.xy) + uTextureOffset;
	vec2 uvCenter = uTextureOffset * abs(uTextureTransformOpacity.xy) + abs(uTextureTransformOpacity.xy) * 0.5;
	vec2 delta = vTexture - uvCenter;
	vTexture = vTexture + delta * uTextureClip;

	gl_Position = instanceModelMatrix[gl_InstanceID] * uModelMatrix * totalLocalPos;
}