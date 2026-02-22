#version 400 core

in		vec4 vPosition;
in		vec3 vNormal;

uniform vec3 uColorEmissive;
uniform vec3 uColorAmbient;
uniform vec3 uColor;

layout(location = 0) out vec3 albedo; //rgb8
layout(location = 1) out vec2 normal; 
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

vec3 hueShift(vec3 color, float hue)
{
	const vec3 k = vec3(0.57735, 0.57735, 0.57735);
	float cosAngle = cos(hue);
	return vec3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
}

float median(float r, float g, float b) 
{
    return max(min(r, g), min(max(r, g), b));
}

float median(vec3 rgb)
{
    return max(min(rgb.r, rgb.g), min(max(rgb.r, rgb.g), rgb.b));
}

void main()
{
	float m = median(vNormal);
	const float factor = 0.25;

	albedo = uColorAmbient * hueShift(uColor, m * factor);
	normal = vec2(0.0);
	idShadowCaster = vec3(1.0, 1.0, 1.0 / 255.0); 
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
    emissive = hueShift(uColorEmissive.xyz, m * factor) * 0.5;
}