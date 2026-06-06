#version 400 core

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

uniform vec3 uColor;

void main()
{
	color = vec4(uColor, 1.0);
	bloom = vec4(0.0, 0.0, 0.0, 1.0);
}