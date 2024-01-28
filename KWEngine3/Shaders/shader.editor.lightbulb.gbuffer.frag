#version 400 core

layout(location = 0) out vec3 albedo;
layout(location = 1) out vec3 normal;
layout(location = 3) out ivec2 idShadowCaster;

uniform vec4 uColorTint;
uniform int uId;
uniform vec3 uCamLAV;

void main()
{
	albedo = vec3(uColorTint.xyz * (uColorTint.w * 0.5));
	normal = -uCamLAV;
	idShadowCaster = ivec2(uId, 0);
}
