#version 400 core

in		vec2 vTexture;

uniform sampler2D uTextureAlbedo;

void main()
{
	if(texture(uTextureAlbedo,vTexture).w <= 0)
		discard;
}