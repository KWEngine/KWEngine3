#version 400 core


in		vec2 gTexture;

uniform sampler2D uTextureAlbedo;

void main()
{
	if(texture(uTextureAlbedo,gTexture).w <= 0)
		discard;
}