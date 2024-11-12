#version 400 core
 
layout(location = 0) in vec3 aPosition;

out		vec4 vNDC;

void main()
{
		vNDC = vec4(aPosition.xy * 2.0, 1.0, 1.0);
		gl_Position = vNDC; 
}