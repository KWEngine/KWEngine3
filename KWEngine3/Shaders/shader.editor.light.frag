#version 400 core

layout(location = 0) out vec4 albedo;

uniform vec3 uColorTint;

void main()
{
	albedo = vec4(uColorTint, 1.0);
}
