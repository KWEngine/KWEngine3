#version 400 core
 
in		vec3 vTexture;
in      vec2 vTextureEquirectangular;

uniform samplerCube uTexture;
uniform sampler2D uTextureEquirectangular;
uniform int uSkyboxType;
uniform vec3 uColorAmbient;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
	if(uSkyboxType == 0)
	{
		color = vec4(uColorAmbient * textureLod(uTexture, vTexture, 0.0).xyz, 1.0);
	}
	else
	{
		vec2 uv = vec2(1.0 - vTextureEquirectangular.x, vTextureEquirectangular.y);
		color = vec4(uColorAmbient * textureLod(uTextureEquirectangular, uv, 0.0).xyz, 1.0);
	}
	bloom = vec4(0.0);
}