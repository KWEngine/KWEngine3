#version 400

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
in vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColor;
uniform vec4 uBloom;

void main()
{
	vec2 texSize = textureSize(uTexture, 0) * 0.5;
	vec2 textureCoordsScreen = gl_FragCoord.xy / texSize;
	float sampled = texture(uTexture, textureCoordsScreen).x;
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