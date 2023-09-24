#version 400 core

in		vec4 vPosition;

uniform vec4 uColorEmissive;
uniform vec3 uColorAmbient;

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType;
layout(location = 3) out float id;

void main()
{
	albedo = vec3(uColorAmbient + uColorEmissive.xyz * uColorEmissive.w);
	normal = vec3(0.0, 0.0, 0.0);
	id = 16777216.0;
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
}