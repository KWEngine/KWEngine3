#version 400 core

layout(location = 0) out vec4 fragmentColor;

in float gOpacity;

uniform vec3 uColor;
void main()
{
	fragmentColor = vec4(uColor, gOpacity);
}