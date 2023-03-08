#version 400 core

uniform vec3 uCenter;

void main()
{
	gl_Position = vec4(uCenter, 1.0);
}