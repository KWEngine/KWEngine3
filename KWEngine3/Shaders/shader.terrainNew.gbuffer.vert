#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

/*
uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
*/
uniform vec4 uTextureTransform;

out vec3 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;
//out mat3 vTBN;

void main()
{
	gl_Position = vec4(aPosition, 1.0); //uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vPosition = vec3(aPosition); //uModelMatrix * totalLocalPos;
	vNormal = aNormal; //normalize((uNormalMatrix * totalNormal).xyz);
	vTangent = aTangent;
	vBiTangent = aBiTangent;
	vTexture = (aTexture + uTextureTransform.zw) * uTextureTransform.xy;
	/*
	vec3 vTangent = normalize((uNormalMatrix * totalTangent).xyz);
	vec3 vBiTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTBN = mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz);
	*/
}