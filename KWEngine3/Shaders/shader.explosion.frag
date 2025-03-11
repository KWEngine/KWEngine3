#version 400 core

in		vec4 vPosition;

uniform vec4 uColorEmissive;
uniform vec3 uColorAmbient;

layout(location = 0) out vec3 albedo; //rgb16f
layout(location = 1) out vec3 normal; //rgb16f
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

void main()
{
	albedo = vec3(uColorAmbient + uColorEmissive.xyz * uColorEmissive.w);
	normal = vec3(0.0, 0.0, 0.0);
	idShadowCaster = vec3(1.0, 1.0, 1 / 255.0); 
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
}