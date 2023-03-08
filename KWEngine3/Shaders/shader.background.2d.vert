#version 400 core

layout(location = 0) in	vec3 aPosition;
layout(location = 1) in	vec2 aTexture;

out		vec2 vTexture;

uniform vec4 uTextureScaleAndOffset;
uniform vec2 uTextureClip;
 
void main()
{
	float u = aTexture.x * uTextureClip.x + uTextureScaleAndOffset.z;
	float v = (1.0 - aTexture.y) * uTextureClip.y - uTextureScaleAndOffset.w;
	vTexture = vec2(u,v) * uTextureScaleAndOffset.xy; 
	gl_Position = vec4(aPosition.xy, 1, 1);
}