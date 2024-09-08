#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform vec4 uTextureTransform;
uniform ivec4 uTerrainData;

out vec3 vPosition;
out vec2 vTexture;
out vec2 vTextureHeight;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;

void main()
{
	int tileCountX = uTerrainData.x / uTerrainData.z;
	int tileCountZ = uTerrainData.y / uTerrainData.z;
	float instanceSize = uTerrainData.z;
	float instanceOffsetX = gl_InstanceID % tileCountX * (instanceSize * 0.5) + instanceSize * 0.25 - uTerrainData.x * 0.25;
	float instanceOffsetZ = gl_InstanceID / tileCountX * (instanceSize * 0.5) + instanceSize * 0.25 - uTerrainData.y * 0.25;

	vec3 offset = vec3(instanceOffsetX, 0.0, instanceOffsetZ);
	gl_Position = vec4(aPosition + offset, 1.0);
	vPosition = aPosition + offset; 
	vNormal = aNormal;
	vTangent = aTangent;
	vBiTangent = aBiTangent;

	float texX = aTexture.x / tileCountX + gl_InstanceID * (1.0 / tileCountX);
	float texZ = aTexture.y / tileCountZ + (gl_InstanceID / tileCountX) * (1.0 / tileCountZ);
	vTexture = vec2(
		(texX + uTextureTransform.z) * uTextureTransform.x,
		(texZ + uTextureTransform.w) * uTextureTransform.y
		);
	vTextureHeight = vec2(texX, texZ);
}