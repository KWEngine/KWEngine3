#version 400 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexture;

uniform mat4 uViewProjectionMatrix;
uniform float uUVOffsetsAndWidths[128 * 4];
uniform samplerBuffer uGlyphInfo; //r=left, g=right, b=top, a=bottom
uniform mat4 uViewProjectionMatrixShadowMap[3];
uniform mat4 uViewProjectionMatrixShadowMapOuter[3];
uniform vec4 uPositionAndOffset;
uniform vec4 uScale;
uniform vec4 uRotation;
uniform float uAdvances[128];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec4 vShadowCoord[3];
out vec4 vShadowCoordOuter[3];


bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 4 || gl_VertexID == 5;
}

bool isTopVertex()
{
	return gl_VertexID == 2 || gl_VertexID == 3 || gl_VertexID == 4;
}

vec3 rotate_vector(vec3 v, vec4 q)
{
    vec3 t = 2.0 * cross(q.xyz, v);
    return v + q.w * t + cross(q.xyz, t);
}


void main()
{
	vec4 glyphInfo = texelFetch(uGlyphInfo, gl_InstanceID); //r=left, g=right, b=top, a=bottom

    float x;
	float y;

	if(isLeftVertex())
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID * 4 + 0];
		x = uAdvances[gl_InstanceID] + glyphInfo.r;
	}
	else
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID * 4 + 1];
		x = uAdvances[gl_InstanceID] + glyphInfo.g;
	}

	if(isTopVertex())
	{
		vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 3];
		y = (-0.5 + glyphInfo.z);
	}
	else
	{
		vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 2];
		y = (-0.5 + glyphInfo.w);
	}

	vec4 pos = vec4(x + uPositionAndOffset.w, y, 0, 1.0);
	pos.xyz = rotate_vector(pos.xyz * uScale.xyz, uRotation);

	vPosition = vec4(pos.x + uPositionAndOffset.x, pos.y + uPositionAndOffset.y, pos.z + uPositionAndOffset.z, 1.0);

	for(int i = 0; i < 3; i++)
	{
		vShadowCoord[i] = uViewProjectionMatrixShadowMap[i] * vPosition;
		vShadowCoordOuter[i] = uViewProjectionMatrixShadowMapOuter[i] * vPosition;
	}
	vNormal = normalize(rotate_vector(vec3(0, 0, 1), uRotation));
	
	gl_Position = uViewProjectionMatrix * vPosition;
}