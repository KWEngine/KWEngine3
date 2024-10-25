#version 400 core

layout(location = 0) out vec4 color;

uniform samplerCube uTexture;
uniform vec3 uCameraLookAtVector;

void main()
{
	vec4 color3D = texture(uTexture, uCameraLookAtVector);
	color = color3D;
}