#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 128) out; 

uniform mat4 uModelViewProjection;
uniform int uVertexCount;
uniform vec3 uVertexPositions[128];

void main()
{
    for(int i = 0; i <= uVertexCount; i++)
    {
        gl_Position = uModelViewProjection * (vec4(uVertexPositions[i % uVertexCount], 1.0));
        EmitVertex();
    }
    EndPrimitive();
}