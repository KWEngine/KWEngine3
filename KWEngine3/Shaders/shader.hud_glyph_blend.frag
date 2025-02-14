#version 400

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
in vec2 vTexture;

uniform sampler2D uTexture;


float getSampleOpacity()
{
	vec2 texelSize = 1.0 / textureSize(uTexture, 0);
	float sampleSum = 0.0;
	float fsample;
	int isample;

	// center texel:
	fsample = texture(uTexture, vTexture + vec2(texelSize.x * +0.0, texelSize.y * +0.0)).x;
	isample = int(round(fsample * 255.0));
	sampleSum += (isample % 2) * 8;

	// upper left texel:
	fsample = texture(uTexture, vTexture + vec2(texelSize.x * -1.0, texelSize.y * +1.0)).x;
	isample = int(round(fsample * 255.0));
	sampleSum += isample % 2;

	// upper right texel:
	fsample = texture(uTexture, vTexture + vec2(texelSize.x * +1.0, texelSize.y * +1.0)).x;
	isample = int(round(fsample * 255.0));
	sampleSum += isample % 2;

	// lower left texel:
	fsample = texture(uTexture, vTexture + vec2(texelSize.x * -1.0, texelSize.y * -1.0)).x;
	isample = int(round(fsample * 255.0));
	sampleSum += isample % 2;

	// lower right texel:
	fsample = texture(uTexture, vTexture + vec2(texelSize.x * +1.0, texelSize.y * -1.0)).x;
	isample = int(round(fsample * 255.0));
	sampleSum += isample % 2;


	return sampleSum / 12.0;
}

void main()
{
	float opacity = getSampleOpacity();

	if(opacity > 0)
	{
		color = vec4(1.0, 1.0, 1.0, opacity);
		bloom = vec4(0.0);
	}
	else
	{
		discard;
	}
}