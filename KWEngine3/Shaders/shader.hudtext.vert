#version 400 core
 
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform float uUVOffsetsAndWidths[128 * 4];
uniform float uCursorBounds[4]; // width, top, bottom, <empty>
uniform samplerBuffer uGlyphInfo; //r=advance, g=widths, b=top, a=bottom
uniform float uOffset;
uniform vec4 uCursorInfo; // x=behaviour, y=worldtime, z=blinkspeed, w=advanceToCursor

bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 4 || gl_VertexID == 5;
}

bool isTopVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 1 || gl_VertexID == 5;
}

void main()
{
	vec2 p = aPosition;
	float left = 0.0;
	float y = 0.0;

	if(uCursorInfo.x != 0)
	{
		left = p.x * uCursorBounds[0] + uCursorInfo.w;
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
			y = -uCursorBounds[1];
		}
		else
		{
			vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 2];
			y = -uCursorBounds[2];
		}
	}
	else
	{
		vec4 glyphInfo = texelFetch(uGlyphInfo, gl_InstanceID); //r=advance, g=widths, b=top, a=bottom
		left = p.x * glyphInfo.y + glyphInfo.y * 0.5 + glyphInfo.x;
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
			y = -glyphInfo.z;
		}
		else
		{
			vTexture.y = uUVOffsetsAndWidths[gl_InstanceID * 4 + 2];
			y = -glyphInfo.w;
		}
	}
	
	

	vec4 pos = vec4(left, y, 0.0, 1.0);

	vec4 posModel = vec4((uModelMatrix * pos).xyz, 1.0);
	posModel.x += uOffset;
	gl_Position = (uViewProjectionMatrix * posModel).xyww;
}