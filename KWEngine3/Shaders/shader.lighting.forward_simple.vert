#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 5) in	uvec3 aBoneIds;
layout(location = 6) in	vec3 aBoneWeights;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform vec4 uTextureTransform;
uniform vec2 uTextureClip;
uniform int uUseAnimations;
uniform mat4 uBoneTransforms[128];

out vec2 vTexture;

void main()
{
	vec4 totalLocalPos = vec4(0.0);
	
	if(uUseAnimations > 0)
	{	
		for(int i = 0; i < 3; i++)
		{
			totalLocalPos += aBoneWeights[i] * uBoneTransforms[aBoneIds[i]] * vec4(aPosition, 1.0);
		}
	}
	else
	{
		totalLocalPos = vec4(aPosition, 1.0);
	}

	gl_Position = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vTexture = (vec2(uTextureTransform.x < 0 ? 1.0 - aTexture.x : aTexture.x, uTextureTransform.y < 0 ? 1.0 - aTexture.y : aTexture.y) + uTextureTransform.zw) * abs(uTextureTransform.xy);
	vec2 uvCenter = uTextureTransform.zw * abs(uTextureTransform.xy) + abs(uTextureTransform.xy) * 0.5;
	vec2 delta = vTexture - uvCenter;
	vTexture = vTexture + delta * uTextureClip;
}