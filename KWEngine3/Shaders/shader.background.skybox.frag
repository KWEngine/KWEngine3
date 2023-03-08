#version 400 core
 
in		vec3 vTexture;

uniform samplerCube uTexture;
uniform vec3 uColorAmbient;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
	color = vec4(uColorAmbient * texture(uTexture, vTexture).xyz, 1.0);
	bloom = vec4(0.0);
}