#version 400 core

layout(location = 1) out vec4 albedo;
layout(location = 2) out vec4 normalId;

uniform vec3 uColorTint;
uniform int uId;

void main()
{
	albedo = vec4(uColorTint, 1.0);
	normalId = vec4(0,0,0, float(uId));
}
