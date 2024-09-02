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
	int tileCountX = int(uTerrainData.x / uTerrainData.z);
	int tileCountZ = int(uTerrainData.y / uTerrainData.z);
	float instanceSize = uTerrainData.z;
	float offsetXUneven = (int(tileCountX) + 1) % 2; // * instanceSize * 0.5;
	float offsetZUneven = (int(tileCountZ) + 1) % 2; // * instanceSize * 0.5;

	float instanceOffsetX = ((gl_InstanceID % tileCountX) - offsetXUneven) * instanceSize;
	float instanceOffsetZ = ((gl_InstanceID / tileCountX) - offsetZUneven) * instanceSize;


	//vec3 adding = vec3(instanceOffsetX, gl_InstanceID * 0.1, instanceOffsetZ);
	vec3 adding = vec3(instanceOffsetX, 0.0 * gl_InstanceID, instanceOffsetZ);
	gl_Position = vec4(aPosition + adding, 1.0);
	vPosition = aPosition + adding; 
	vNormal = aNormal;
	vTangent = aTangent;
	vBiTangent = aBiTangent;
	vTexture = (aTexture + uTextureTransform.zw) * uTextureTransform.xy;

}