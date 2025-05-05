#version 400 core

layout(triangles, invocations = 2) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 uViewProjectionMatrix[2];

out		float gZ;
out		float gW;

void main()
{
    
	for (int i = 0; i < 3; i++)
    {
        vec4 transformedPosition = uViewProjectionMatrix[gl_InvocationID] * gl_in[i].gl_Position;
	    gZ = transformedPosition.z;
	    gW = transformedPosition.w;

        gl_Position = transformedPosition;
        gl_Layer = gl_InvocationID;    
        EmitVertex();
    }
    EndPrimitive();
}