#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 2) out; 

uniform mat4 uModelViewProjection;
uniform int uType;

void main()
{
    

    vec4 centerClipSpace = uModelViewProjection * gl_in[0].gl_Position;

    if(uType == 0)
    {
        gl_Position = centerClipSpace;
        EmitVertex();

        gl_Position = uModelViewProjection * vec4(100000, 0, 0, 1);
        EmitVertex();
        EndPrimitive();
    }
    else if(uType == 1)
    {  
       gl_Position = centerClipSpace;
        EmitVertex();
        gl_Position = uModelViewProjection * vec4(0, 100000, 0, 1);
        EmitVertex();
        EndPrimitive();
    }
    else if(uType == 2)
    {  
        gl_Position = centerClipSpace;
        EmitVertex();
        gl_Position = uModelViewProjection * vec4(0, 0, 100000, 1);
        EmitVertex();
        EndPrimitive();
    }
}