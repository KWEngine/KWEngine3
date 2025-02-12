#version 400

layout(location = 0) out float r8;
layout(location = 1) out vec4 color;

uniform vec4 uColorTint;
 
void main()
{
	r8 = 1.0 / 255.0;
	color = uColorTint;
}