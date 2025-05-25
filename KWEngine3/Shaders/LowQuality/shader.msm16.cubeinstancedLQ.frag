#version 400 core
in vec2 gTexture;
in vec4 gPosition;

uniform vec3 uLightPosition;
uniform vec2 uNearFar;

uniform sampler2D uTextureAlbedo;

void main()
{
	if(texture(uTextureAlbedo,gTexture).w <= 0.0)
		discard;

	gl_FragDepth = length(gPosition.xyz - uLightPosition) / uNearFar.y;
}  