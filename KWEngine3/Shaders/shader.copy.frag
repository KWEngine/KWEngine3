#version 400 core

in vec2 vTexture;

layout(location = 0) out vec4 color;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureBloom;
uniform vec4 uFadeColor;

void main()
{
	vec3 scene = texture(uTextureAlbedo, vTexture).xyz + texture(uTextureBloom, vTexture).xyz;
	vec3 fade = uFadeColor.xyz;
	color = vec4(scene * uFadeColor.w + fade * (1.0 - uFadeColor.w), 1.0);
}