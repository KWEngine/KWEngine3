#version 400

layout(location = 0) in	vec2 aPosition;

uniform mat4 uModel;
uniform mat4 uViewProjection;

void main()
{
	gl_Position = uViewProjection * uModel * vec4(aPosition.xy, 0, 1);
}