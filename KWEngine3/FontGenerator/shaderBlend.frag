#version 400

layout(location = 0) out vec4 color;
 
in vec2 vTexture;

uniform sampler2D uTextureGlyphs;

void main()
{
	float sampled = texture(uTextureGlyphs, vTexture).x;
	int iSample = int(sampled * 255.0);

	if(iSample % 2 == 1)
	{
		color = vec4(1.0);
	}
	else
	{
		discard;
	}
}