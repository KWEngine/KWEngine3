#version 400

layout(location = 0) in	vec2 aPosition;

uniform vec3 uOffsetScale;
uniform mat4 uViewProjection;

void main()
{
	gl_Position = uViewProjection * vec4(aPosition.xy * uOffsetScale.z + uOffsetScale.xy, 0.0, 1.0);
}