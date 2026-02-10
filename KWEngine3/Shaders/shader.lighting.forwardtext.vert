#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform float uUVOffsetsAndWidths[128 * 4];
uniform samplerBuffer uGlyphInfo; //r=advance, g=widths, b=top, a=bottom
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

bool isTopVertex()
{
	return gl_VertexID == 1 || gl_VertexID == 4 || gl_VertexID == 7 || gl_VertexID == 8 || gl_VertexID == 10 || gl_VertexID == 11;
}

void main()
{
	vec4 glyphInfo = texelFetch(uGlyphInfo, gl_InstanceID); //r=advance, g=widths, b=top, a=bottom

	vec4 pos = vec4(aPosition, 1.0);
    pos.x *= glyphInfo.g;
    float left = pos.x + glyphInfo.g * 0.5 + glyphInfo.r;
	float y = 0.0;

	if(isLeftVertex())
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID * 4 + 0];
	}
	else
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID * 4 + 1];
	}

	if(isTopVertex())
	{
		vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 3];
		y = -glyphInfo.w;
	}
	else
	{
		vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 2];
		y = -glyphInfo.z;
	}

	pos = vec4(left, y, pos.z, 1.0);

	gl_Position = uViewProjectionMatrix * uModelMatrix * pos;
	vPosition = uModelMatrix * pos;
	for(int i = 0; i < 3; i++)
	{
		vShadowCoord[i] = uViewProjectionMatrixShadowMap[i] * uModelMatrix * pos;
		vShadowCoordOuter[i] = uViewProjectionMatrixShadowMapOuter[i] * uModelMatrix * pos;
	}
	vNormal = normalize((uModelMatrix * vec4(aNormal, 0.0)).xyz);
	
}