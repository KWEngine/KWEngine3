#version 400

layout(location = 0) in	vec2 aPosition;

uniform vec2 uModelInternal;
uniform mat4 uModelExternal;
uniform mat4 uViewProjection;

out vec3 vBarycentric;

void main()
{
	if(gl_VertexID % 3 == 0)
	{
		vBarycentric = vec3(1.0, 0.0, 0.0);
	}
	else if(gl_VertexID % 3 == 1)
	{
		vBarycentric = vec3(0.0, 1.0, 0.0);
	}
	else if(gl_VertexID % 3 == 2)
	{
		vBarycentric = vec3(0.0, 0.0, 1.0);
	}

	gl_Position = uViewProjection * uModelExternal * vec4(aPosition.xy + uModelInternal, 0, 1);
}