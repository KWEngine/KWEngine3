#version 400 core
 
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uModelViewProjectionMatrix;
uniform int uOffset;
uniform int uIsText;
 
void main()
{
	if(uIsText > 0)
	{
		vTexture.x = aTexture.x / 256.0 + (uOffset / 256.0);
	}
	else
	{
		vTexture.x = aTexture.x;
	}

	vTexture.y = aTexture.y;
	gl_Position = (uModelViewProjectionMatrix * vec4(aPosition, 1.0)).xyww; 
}