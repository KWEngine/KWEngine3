#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

uniform mat4 uViewProjectionMatrix;

out vec2 vTexture;

void main()
{
	vTexture = aTexture;
	gl_Position = uViewProjectionMatrix * vec4(aPosition, 1.0);
}