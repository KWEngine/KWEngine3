#version 400

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
in vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColor;
uniform vec4 uBloom;

void main()
{
	float sampled = texture(uTexture, vTexture).x;
	int iSample = int(sampled * 255.0);

	if(iSample % 2 == 1)
	{
		color = uColor;
		bloom = uBloom;
	}
	else
	{
		discard;
	}
}