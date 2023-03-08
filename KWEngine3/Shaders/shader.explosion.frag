#version 400 core

in		vec4 vPosition;

uniform vec4 uColorEmissive;
uniform vec3 uColorAmbient;

layout(location = 0) out vec4 positionDepth;
layout(location = 1) out vec4 albedo;
layout(location = 2) out vec4 normalId;
layout(location = 3) out vec4 csDepthMetallicRoughnessMetallicType;
layout(location = 4) out vec4 emissive;

void main()
{
	positionDepth = vec4(vPosition.xyz, gl_FragCoord.z);
	albedo = vec4(uColorAmbient + uColorEmissive.xyz, 1.0);
	emissive = vec4(uColorEmissive.xyz * uColorEmissive.w, 1.0);
	normalId = vec4(0.0, 0.0, 0.0, 16777216.0);
	csDepthMetallicRoughnessMetallicType = vec4(gl_FragCoord.z, 0.0, 1.0, 0.0);
}