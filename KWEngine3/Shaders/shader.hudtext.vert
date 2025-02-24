#version 400 core
 
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;
out		vec4 vNDC;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform vec2 uUVOffsetsAndWidths[128];
uniform float uAdvanceList[128];
uniform float uWidths[128];
uniform int uMode;
uniform int uTextAlign; // 0 = left, 1 = center, 2 = right
 

bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 4 || gl_VertexID == 5;
}

void main()
{
	vec2 p = aPosition;

	p.x *= uWidths[gl_InstanceID];
	if(isLeftVertex())
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].x;
	}
	else
	{
		vTexture.x = uUVOffsetsAndWidths[gl_InstanceID].y;
	}
	vTexture.y = aTexture.y;

	float left = p.x + 0.25 + uAdvanceList[gl_InstanceID] * 1.0;
	vec4 pos = vec4(left, p.y, 0.0, 1.0);

	float offset = 0;
	if(uTextAlign == 0) // left
	{
		offset = 0.0;
	}
	else if(uTextAlign == 1) // center
	{
		
	}
	else // right
	{

	}

	pos.x += offset;
	vNDC = uViewProjectionMatrix * uModelMatrix * pos;
	gl_Position = vNDC.xyww; 
	//gl_Position = vNDC; 
	
}