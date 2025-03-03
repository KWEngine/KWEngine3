#version 400 core

layout(location = 0) in vec3 aPosition;

//uniform vec4 uTextureOffset; // left, right, top, bottom

layout (std140) uniform uBlockIndex1
{
	vec4 instanceData1[64]; // quad position and scale
};
layout (std140) uniform uBlockIndex2
{
	vec4 instanceData2[64]; // quad texture coordinates
};

out vec4 vPosition;
out vec2 vTexture;
flat out int vInstanceID;

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

	vInstanceID = gl_InstanceID;
	vec4 positionAndScale = instanceData1[gl_InstanceID];
	vec4 uvCoordinates = instanceData2[gl_InstanceID];
	vTexture = vec2(isLeftVertex() ? uvCoordinates.x : uvCoordinates.y, isTopVertex() ? uvCoordinates.z : uvCoordinates.w);
	gl_Position = vec4(aPosition.xy * 0.5 * positionAndScale.xy + positionAndScale.zw, 0.0, 1.0);


/*
	vTexture = vec2(isLeftVertex() ? uTextureOffset.x : uTextureOffset.y, isTopVertex() ? uTextureOffset.z : uTextureOffset.w);
	gl_Position = vec4(aPosition.xy, 0.0, 1.0);
*/
}