#version 400

layout(location = 0) out float color;
 
in vec2 vTexture;

uniform sampler2D uTextureGlyphs;

void main()
{
	float sampled = texture(uTextureGlyphs, vTexture).x;
	int iSample = int(round(sampled * 255.0));

	if(iSample % 2 == 1)
	{
		color = 1.0;
	}
	else
	{
		discard;
	}
}