#version 400 core
in		vec2 vTexture;

uniform vec4        uColorTint;
uniform sampler2D   uTexture;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
	color = ((texture(uTexture, vTexture)) * vec4(uColorTint.xyz, 1.0)) * uColorTint.w;
	
	bloom.x = color.x;
	bloom.y = color.y;
	bloom.z = color.z;
	bloom.w = color.w * 0.05;

}