#version 400 core
 
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;
out		vec4 vNDC;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform int uOffsets[256];
uniform int uOffsetCount;
uniform int uMode;
uniform int uTextAlign; // 0 = left, 1 = center, 2 = right
uniform float uCharacterDistance;
uniform vec2 uTextureRepeat;
 
const float letterSize = 128.0;

void main()
{
	vec4 pos = vec4(0);
	if(uMode == 0) // Text
	{
		float offset = 0;
		vTexture.y = aTexture.y;
		vTexture.x = aTexture.x / letterSize + (uOffsets[gl_InstanceID] / letterSize);

		float left = aPosition.x + 0.5 + gl_InstanceID * uCharacterDistance;
		pos = vec4(aPosition, 1.0);
		pos.x = left;

		if(uTextAlign == 0) // left
			offset = 0.0;
		else if(uTextAlign == 1) // center
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) * 0.5 - 0.5;
		}
		else // right
			offset = -((uOffsetCount - 1) * uCharacterDistance) - 1.0;

		pos.x += offset;
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}
	else if(uMode == 3) // input type: pipe
	{
		float offset = 0;
		vTexture.y = 1.0 - aTexture.y;
		vTexture.x = aTexture.x / letterSize + (uOffsets[gl_InstanceID] / letterSize);

		float left = aPosition.x + 0.5 + gl_InstanceID * uCharacterDistance;
		pos = vec4(aPosition, 1.0);
		pos.x = left;

		float cursorOffset = 0.0;
		if(uTextAlign == 0)
		{
			// left
			offset = 0.0;
			cursorOffset = -uCharacterDistance * 0.5;
		}
		else if(uTextAlign == 1) // center
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) * 0.5 - 0.5;
		}
		else // right
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) - 1.0;
			cursorOffset = +uCharacterDistance * 0.5;
		}

		
		pos.x += (offset + cursorOffset);
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}
	else if(uMode == 4) // input type: underscore
	{
		float offset = 0;
		vTexture.y = 1.0 - aTexture.y;
		vTexture.x = aTexture.x / letterSize + (uOffsets[gl_InstanceID] / letterSize);

		float left = aPosition.x + 0.5 + gl_InstanceID * uCharacterDistance;
		pos = vec4(aPosition, 1.0);
		pos.x = left;

		float cursorOffset = 0.0;
		if(uTextAlign == 0) // left
		{
			offset = 0.0;
			cursorOffset = -uCharacterDistance * 0.05;
		}
		else if(uTextAlign == 1) // center
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) * 0.5 - 0.5;
			cursorOffset = uCharacterDistance * 0.5 - uCharacterDistance * 0.05;
		}
		else // right
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) - 1.0;
			cursorOffset = -uCharacterDistance * 0.05;
		}

		pos.x += (offset + cursorOffset);
		pos.y += 0.1f;
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}
	else if(uMode == 5) // input type: block
	{
		float offset = 0;
		vTexture.y = 1.0 - aTexture.y;
		vTexture.x = aTexture.x / letterSize + (uOffsets[gl_InstanceID] / letterSize);

		float left = aPosition.x + 0.5 + gl_InstanceID * uCharacterDistance;
		pos = vec4(aPosition, 1.0);
		pos.x = left;

		float cursorOffset = 0.0;
		if(uTextAlign == 0) // left
		{
			offset = 0.0;
			cursorOffset = -uCharacterDistance * 0.05;
		}
		else if(uTextAlign == 1) // center
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) * 0.5 - 0.5;
			cursorOffset = uCharacterDistance * 0.5 - uCharacterDistance * 0.05;
		}
		else // right
		{
			offset = -((uOffsetCount - 1) * uCharacterDistance) - 1.0;
			cursorOffset = -uCharacterDistance * 0.05;
		}

		pos.x += (offset + cursorOffset);
		pos.y += 0.0f;
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}

	else if(uMode == 1) // Image
	{
		vTexture.x = aTexture.x * uTextureRepeat.x;
		vTexture.y = aTexture.y * uTextureRepeat.y;
		pos = vec4(aPosition, 1.0);
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}
	else
	{
		vTexture.x = aTexture.x * uTextureRepeat.x;
		vTexture.y = 1.0 - aTexture.y * uTextureRepeat.y;
		pos = vec4(aPosition, 1.0);
		vNDC = uViewProjectionMatrix * uModelMatrix * pos;
		gl_Position = vNDC.xyww; 
	}
}