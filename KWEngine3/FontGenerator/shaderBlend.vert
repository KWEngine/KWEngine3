#version 400

layout(location = 0) in	vec2 aPosition;
layout(location = 1) in	vec2 aTexture;

out vec2 vTexture;

void main()
{
	vTexture = aTexture;
	gl_Position = vec4(aPosition.xy, 0, 1);
}