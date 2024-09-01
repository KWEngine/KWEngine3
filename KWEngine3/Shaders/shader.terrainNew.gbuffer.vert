#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform vec4 uTextureTransform;

out vec3 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;
//out mat3 vTBN;

void main()
{
	gl_Position = vec4(aPosition, 1.0);
	vPosition = vec3(aPosition); 
	vNormal = aNormal;
	vTangent = aTangent;
	vBiTangent = aBiTangent;
	vTexture = (aTexture + uTextureTransform.zw) * uTextureTransform.xy;

}