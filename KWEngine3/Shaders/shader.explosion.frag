#version 400 core

in		vec4 vPosition;

uniform vec3 uColorEmissive;
uniform vec3 uColorAmbient;
uniform vec3 uColor;
uniform vec2 uRoughnessMetallic;

layout(location = 0) out vec3 albedo; //rgb8
layout(location = 1) out vec3 normal; 
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

void main()
{
	albedo = uColorAmbient * uColor;
	normal = vec3(0.0, 0.0, 0.0);
	idShadowCaster = vec3(1.0, 1.0, 1 / 255.0); 
	metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, max(uRoughnessMetallic.x, 0.0001), 0.0);
    emissive = uColorEmissive.xyz * 0.5;
}