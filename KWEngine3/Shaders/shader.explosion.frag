#version 400 core

in		vec4 vPosition;

uniform vec4 uColorEmissive;
uniform vec3 uColorAmbient;

layout(location = 0) out vec4 positionId;
layout(location = 1) out vec4 albedo;
layout(location = 2) out vec4 normalDepth;
layout(location = 3) out vec3 metallicRoughnessMetallicType;
layout(location = 4) out vec4 emissive;

void main()
{
	positionId = vec4(vPosition.xyz, 16777216.0);
	albedo = vec4(uColorAmbient + uColorEmissive.xyz, 1.0);
	emissive = vec4(uColorEmissive.xyz * uColorEmissive.w, 1.0);
	normalDepth = vec4(0.0, 0.0, 0.0, gl_FragCoord.z);
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
}