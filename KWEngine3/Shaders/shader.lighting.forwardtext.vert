#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform vec2 uUVOffsetsAndWidths[128];
uniform float uAdvanceList[128];
uniform float uWidths[128];
uniform mat4 uViewProjectionMatrixShadowMap[3];
uniform mat4 uViewProjectionMatrixShadowMapOuter[3];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec4 vShadowCoord[3];
out vec4 vShadowCoordOuter[3];


bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 4 || gl_VertexID == 5 || gl_VertexID == 6 || gl_VertexID == 10 || gl_VertexID == 11;
}

void main()
{
	vec4 pos = vec4(aPosition, 1.0);
    pos.x *= uWidths[gl_InstanceID];
    float left = pos.x + uWidths[gl_InstanceID] * 0.5 + uAdvanceList[gl_InstanceID];
	pos = vec4(left, pos.y, pos.z, 1.0);
	
	if(isLeftVertex())
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].x;
	}
	else
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].y;
	}
	vTexture.y = aTexture.y;


	gl_Position = uViewProjectionMatrix * uModelMatrix * pos;
	vPosition = uModelMatrix * pos;
	for(int i = 0; i < 3; i++)
	{
		vShadowCoord[i] = uViewProjectionMatrixShadowMap[i] * uModelMatrix * pos;
		vShadowCoordOuter[i] = uViewProjectionMatrixShadowMapOuter[i] * uModelMatrix * pos;
	}
	vNormal = normalize((uModelMatrix * vec4(aNormal, 0.0)).xyz);
	
}