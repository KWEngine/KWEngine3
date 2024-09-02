#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform vec4 uTextureTransform;
uniform vec4 uTerrainData;

out vec3 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vTangent;
out vec3 vBiTangent;

void main()
{
	int tileCountX = int(uTerrainData.x / uTerrainData.z); // 64 / 16 = 4
	int tileCountZ = int(uTerrainData.y / uTerrainData.z); // 64 / 16 = 4
	float instanceSize = uTerrainData.z;				   // 16
	float offsetXUneven = int(tileCountX + 1) % 2;       // 5 % 2 = 1
	float offsetZUneven = int(tileCountZ + 1) % 2;       // 5 % 2 = 1

	float instanceOffsetX = gl_InstanceID % tileCountX * (instanceSize * 0.5) + offsetXUneven * instanceSize * 0.25 - uTerrainData.x * 0.25;
	float instanceOffsetZ = gl_InstanceID / tileCountX * (instanceSize * 0.5) + offsetZUneven * instanceSize * 0.25 - uTerrainData.y * 0.25;


	//vec3 adding = vec3(instanceOffsetX, gl_InstanceID * 0.1, instanceOffsetZ);
	vec3 adding = vec3(instanceOffsetX, 0.0 * gl_InstanceID, instanceOffsetZ);
	gl_Position = vec4(aPosition + adding, 1.0);
	vPosition = aPosition + adding; 
	vNormal = aNormal;
	vTangent = aTangent;
	vBiTangent = aBiTangent;
	vTexture = vec2(
		(aTexture.x + uTextureTransform.z) * uTextureTransform.x,
		(aTexture.y + uTextureTransform.w) * uTextureTransform.y
		);
}