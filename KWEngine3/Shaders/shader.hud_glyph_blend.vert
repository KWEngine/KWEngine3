#version 400

layout(location = 0) in	vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out vec2 vTexture;
//uniform mat4 uModel;
//uniform mat4 uViewProjection;

void main()
{
	vTexture = aTexture;
	gl_Position = vec4(aPosition.xyz, 1);
}