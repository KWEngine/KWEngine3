#version 400 core
 
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec2 vTexture;
out		vec4 vNDC;

uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform int uTextAlign; // 0 = left, 1 = center, 2 = right
uniform vec2 uTextureRepeat;
 
void main()
{
	vec4 pos = vec4(aPosition, 1.0);
	vTexture.x = aTexture.x * uTextureRepeat.x;
	vTexture.y = aTexture.y * uTextureRepeat.y;
	vNDC = uViewProjectionMatrix * uModelMatrix * pos;
	gl_Position = vNDC.xyww;	
}