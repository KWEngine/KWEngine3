#version 400

layout(location = 0) in	vec2 aPosition;

uniform mat4 uModel;
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

	gl_Position = uViewProjection * uModel * vec4(aPosition.xy, 0, 1);
}