#version 400 core

in		vec4 vPosition;

uniform vec3 uColorEmissive;
uniform vec3 uColorAmbient;
uniform vec3 uColor;
uniform vec2 uRoughnessMetallic;

layout(location = 0) out vec3 albedo; //rgb8
layout(location = 1) out vec2 normal; 
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8
layout(location = 4) out vec3 emissive; // rgb8

vec2 encodeNormalToRG16F(vec3 n) {
    n /= (abs(n.x) + abs(n.y) + abs(n.z));

    vec2 enc = (n.z >= 0.0) 
        ? n.xy 
        : (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);

    return enc;
}

vec2 encode16BitUintTo8Bit(uint value16) 
{
    float low8 = float(value16 & 0xFFu); // Untere 8 Bit
    float high8 = float((value16 >> 8) & 0xFFu); // Obere 8 Bit
    return vec2(high8 / 255.0, low8 / 255.0);
}

void main()
{
	albedo = uColorAmbient * uColor;
	normal = encodeNormalToRG16F(vec3(0.0));
	idShadowCaster = vec3(1.0, 1.0, 1.0 / 255.0); 
	metallicRoughnessMetallicType = vec3(uRoughnessMetallic.y, max(uRoughnessMetallic.x, 0.00001), 0.0);
    emissive = uColorEmissive.xyz * 0.5;
}