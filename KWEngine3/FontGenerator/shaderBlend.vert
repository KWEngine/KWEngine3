#version 400

layout(location = 0) in	vec2 aPosition;

out vec2 vTexture;

void main()
{
	if(gl_VertexID == 0)
	{
		vTexture = vec2(0,0);
	}
	else if(gl_VertexID == 1)
	{
		vTexture = vec2(1,0);
	}
	else if(gl_VertexID == 2)
	{
		vTexture = vec2(1,1);
	}
	else if(gl_VertexID == 3)
	{
		vTexture = vec2(1,1);
	}
	else if(gl_VertexID == 4)
	{
		vTexture = vec2(0,1);
	}
	else if(gl_VertexID == 5)
	{
		vTexture = vec2(0,0);
	}

	gl_Position = vec4(aPosition.xy, 0, 1);
}