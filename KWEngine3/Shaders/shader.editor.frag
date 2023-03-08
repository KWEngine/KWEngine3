#version 400 core

layout(location = 0) out vec4 color;

uniform vec3 uColor;

void main()
{
	color = vec4(uColor, 1.0);
}