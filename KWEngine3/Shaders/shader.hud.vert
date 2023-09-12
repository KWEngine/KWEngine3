#version 400 core
 
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform int uOffsets[256];
uniform int uOffsetCount;
uniform int uMode;
uniform int uTextAlign;
uniform float uCharacterDistance;
 
void main()
{
	vec4 pos = vec4(0);
	if(uMode == 0) // Text
	{
		float offset = 0;
		vTexture.y = 1.0 - aTexture.y;
		vTexture.x = aTexture.x / 256.0 + (uOffsets[gl_InstanceID] / 256.0);


		if(uTextAlign == 0) // left
			offset = 0;
		else if(uTextAlign == 1) // center
			offset = -uOffsetCount / 2.0;
		else
			offset = -uOffsetCount;

		pos = vec4(aPosition, 1.0) + vec4(offset + gl_InstanceID * uCharacterDistance, 0, 0, 0);
	}
	else if(uMode == 1) // Image
	{
		vTexture.x = aTexture.x;
		vTexture.y = 1.0 - aTexture.y;
		pos = vec4(aPosition, 1.0);
	}
	
	gl_Position = (uViewProjectionMatrix * uModelMatrix * pos).xyww; 
}