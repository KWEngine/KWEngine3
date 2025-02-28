#version 400 core
 
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform vec2 uUVOffsetsAndWidths[128];
uniform float uAdvanceList[128];
uniform float uWidths[128];
uniform float uOffset;
uniform vec4 uCursorInfo;

bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 4 || gl_VertexID == 5;
}

void main()
{
	vec2 p = aPosition;
	float left = 0.0;

	if(uCursorInfo.x != 0)
	{
		p.x *= uWidths[0]; // 0 = width, 1 = cursorAdvance
		left = p.x + uWidths[1];
	}
	else
	{
		p.x *= uWidths[gl_InstanceID];
		left = p.x + uWidths[gl_InstanceID] * 0.5 + uAdvanceList[gl_InstanceID];
	}
	vec4 pos = vec4(left, p.y, 0.0, 1.0);
	

	if(isLeftVertex())
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].x;
	}
	else
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].y;
	}
	vTexture.y = aTexture.y;

	vec4 posModel = vec4((uModelMatrix * pos).xyz, 1.0);
	posModel.x += uOffset;
	gl_Position = (uViewProjectionMatrix * posModel).xyww;
}