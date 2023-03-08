#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec4 uTextureTransform;

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;
out mat3 vTBN;

void main()
{
	vec4 totalLocalPos = vec4(aPosition, 1.0);
	vec4 totalNormal = vec4(aNormal, 0.0);
	vec4 totalTangent = vec4(aTangent, 0.0);
	vec4 totalBiTangent = vec4(aBiTangent, 0.0);

	gl_Position = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vPosition = uModelMatrix * totalLocalPos;
	vNormal = normalize((uNormalMatrix * totalNormal).xyz);
	vTangent = normalize((uNormalMatrix * totalTangent).xyz);
	vBiTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTexture = (aTexture  + uTextureTransform.zw) * uTextureTransform.xy;
	vTBN = mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz);
}