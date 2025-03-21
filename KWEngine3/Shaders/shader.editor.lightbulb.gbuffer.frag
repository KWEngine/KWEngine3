#version 400 core

layout(location = 0) out vec3 albedo; //R11G11B10f
layout(location = 1) out vec2 normal; //rg8ui
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

uniform vec4 uColorTint;
uniform int uId;
uniform vec3 uCamLAV;

vec2 encodeNormal(vec3 normal) 
{
    normal.xy /= abs(normal.x) + abs(normal.y) + abs(normal.z);
    if (normal.z < 0.0) {
        normal.xy = (1.0 - abs(normal.xy)) * sign(normal.xy);
    }
    return normal.xy * 0.5 + 0.5;
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
