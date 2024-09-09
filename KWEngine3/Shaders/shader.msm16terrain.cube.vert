#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;

uniform ivec4 uTerrainData;

out vec3 vPosition;
out vec3 vNormal;
out vec2 vTextureHeight;

void main()
{
	int tileCountX = uTerrainData.x / uTerrainData.z;
	int tileCountZ = uTerrainData.y / uTerrainData.z;
	float instanceSize = uTerrainData.z;
	float instanceOffsetX = (gl_InstanceID % tileCountX) * (instanceSize * 1.0) - uTerrainData.x * 0.5 + instanceSize * 0.5; 
	float instanceOffsetZ = (gl_InstanceID / tileCountX) * (instanceSize * 1.0) - uTerrainData.y * 0.5 + instanceSize * 0.5;

	vec3 offset = vec3(instanceOffsetX, 0.0, instanceOffsetZ);
	gl_Position = vec4(aPosition + offset, 1.0);
	vPosition = aPosition + offset; 
	vNormal = aNormal;

	float texX = aTexture.x / tileCountX + gl_InstanceID * (1.0 / tileCountX);
	float texZ = aTexture.y / tileCountZ + (gl_InstanceID / tileCountX) * (1.0 / tileCountZ);
	vTextureHeight = vec2(texX, texZ);
}