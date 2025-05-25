#version 400 core

layout(location = 0) in vec3 aPosition;

uniform vec4 uTextureOffset; // left, right, top, bottom

out vec4 vPosition;
out vec2 vTexture;

/*
-1f, -1f | 0,
+1f, -1f | 1,
-1f, +1f | 2,
		 
+1f, -1f | 3,
+1f, +1f | 4,
-1f, +1f | 5
*/

bool isLeftVertex()
{
	return gl_VertexID == 0 || gl_VertexID == 2 || gl_VertexID == 5;
}

bool isTopVertex()
{
	return gl_VertexID == 2 || gl_VertexID == 4 || gl_VertexID == 5;
}

void main()
{
	vTexture = vec2(isLeftVertex() ? uTextureOffset.x : uTextureOffset.y, isTopVertex() ? uTextureOffset.z : uTextureOffset.w);
	gl_Position = vec4(aPosition.xy, 0.0, 1.0);

}