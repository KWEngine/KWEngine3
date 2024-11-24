#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 256) out; 

uniform mat4 uModelViewProjection;
uniform vec3 uPosition0;
uniform vec3 uPosition1;
uniform vec3 uPosition2;
uniform vec3 uPosition3;
uniform vec3 uPositions[256];
uniform int uPositionsCount;
uniform int uIsSector;


void main()
{
    if(uIsSector > 0)
    {
        gl_Position = uModelViewProjection * (vec4(uPosition0, 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uPosition1, 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uPosition2, 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uPosition3, 1.0));
        EmitVertex();
        gl_Position = uModelViewProjection * (vec4(uPosition0, 1.0));
        EmitVertex();

        EndPrimitive();
    }
    else
    {
        for(int i = 0; i < uPositionsCount; i+=3)
        {
            gl_Position = uModelViewProjection * (vec4(uPositions[i+0], 1.0));
            EmitVertex();
            gl_Position = uModelViewProjection * (vec4(uPositions[i+1], 1.0));
            EmitVertex();
            gl_Position = uModelViewProjection * (vec4(uPositions[i+2], 1.0));
            EmitVertex();
            gl_Position = uModelViewProjection * (vec4(uPositions[i+0], 1.0));
            EmitVertex();

            EndPrimitive();
        }
    }
}