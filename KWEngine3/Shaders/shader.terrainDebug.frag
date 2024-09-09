#version 400 core

layout(location = 0) out vec4 color;
uniform int uIsSector;

void main()
{
	if(uIsSector > 0)
	{
		color = vec4(1.0, 0.0, 0.0, 1.0);
	}
	else
	{
		color = vec4(0.75, 0.75, 0.75, 1.0);
	}
}