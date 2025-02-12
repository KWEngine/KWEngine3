#version 400

layout(location = 0) in	vec2 aPosition;

uniform vec2 uModelInternal;
uniform mat4 uModelExternal;
uniform mat4 uViewProjection;

void main()
{
	gl_Position = uViewProjection * uModelExternal * vec4(aPosition.xy + uModelInternal, 0, 1);
}