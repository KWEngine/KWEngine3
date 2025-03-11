#version 400 core

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec2 normal;
layout(location = 3) out vec3 idShadowCaster;

uniform vec4 uColorTint;
uniform int uId;
uniform vec3 uCamLAV;

vec2 encodeNormal(vec3 normal) 
{
    vec2 projected = normal.xy / (1.0 + normal.z);

    vec2 encoded;
    encoded.x = (projected.x + 1.0) * 0.5;
    encoded.y = (projected.y + 1.0) * 0.5;

    return encoded;
}

vec2 encode16BitUintTo8Bit(uint value16) 
{
    float low8 = float(value16 & 0xFFu); // Untere 8 Bit
    float high8 = float((value16 >> 8) & 0xFFu); // Obere 8 Bit
    return vec2(high8 / 255.0, low8 / 255.0);
}

void main()
{
	albedo = vec3(uColorTint.xyz * (uColorTint.w * 0.5));
	normal = encodeNormal(-uCamLAV);
	idShadowCaster = vec3(encode16BitUintTo8Bit(uId), 0);
}
