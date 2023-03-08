#version 400 core

in vec2 vTexture;

layout(location = 0) out vec4 color;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureBloom;

void main()
{
	
	color = texture(uTextureAlbedo, vTexture) + texture(uTextureBloom, vTexture);
}