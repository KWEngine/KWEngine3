#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform int uCharacterOffsets[128];
uniform float uTextOffset;
uniform float uSpread;
uniform mat4 uViewProjectionMatrixShadowMap[3];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec4 vShadowCoord[3];

void main()
{
	vec4 totalLocalPos = vec4(0.0);
	vec4 totalNormal = vec4(0.0);
	
	totalLocalPos = vec4(aPosition, 1.0) - vec4(uTextOffset, 0, 0, 0) + vec4(gl_InstanceID * uSpread, 0, gl_InstanceID * 0.001, 0);
	totalNormal = vec4(aNormal, 0.0);

	gl_Position = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vPosition = uModelMatrix * totalLocalPos;
	for(int i = 0; i < 3; i++)
	{
		vShadowCoord[i] = uViewProjectionMatrixShadowMap[i] * uModelMatrix * totalLocalPos;
	}
	vNormal = normalize((uModelMatrix * totalNormal).xyz);
	vTexture.x = aTexture.x / 256.0 + (uCharacterOffsets[gl_InstanceID] / 256.0);
	vTexture.y = aTexture.y;
}