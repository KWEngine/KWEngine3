#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 6) out; 

uniform mat4 uModelViewProjection;
uniform int uVertexCount;
uniform vec3 uVertexPositions[6];

void main()
{
    if(uVertexCount == 4)
    {
        gl_Position = uModelViewProjection * (vec4(uVertexPositions[0], 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uVertexPositions[1], 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uVertexPositions[2], 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uVertexPositions[3], 1.0));
        EmitVertex();
        EndPrimitive();
    }
    else
    {
        for(int i = 0; i < uVertexCount; i+=3)
        {
            gl_Position = uModelViewProjection * (vec4(uVertexPositions[i], 1.0));
            EmitVertex();
            gl_Position = uModelViewProjection * (vec4(uVertexPositions[i+1], 1.0));
            EmitVertex();
            gl_Position = uModelViewProjection * (vec4(uVertexPositions[i+2], 1.0));
            EmitVertex();
            EndPrimitive();
        }
    }
}