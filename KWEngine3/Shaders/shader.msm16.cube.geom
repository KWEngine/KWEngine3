#version 400 core
layout (triangles) in;
layout (triangle_strip, max_vertices=18) out;

in vec2 vTexture[];

uniform mat4 uViewProjectionMatrix[6];

out vec4 gPosition; // gPosition from GS (output per emitvertex)
out vec2 gTexture;

void main()
{
    for(int face = 0; face < 6; ++face)
    {
        gl_Layer = face; // built-in variable that specifies to which face we render.
        for(int i = 0; i < 3; ++i) // for each triangle vertex
        {
            gPosition = gl_in[i].gl_Position;
            gTexture = vTexture[i];
            gl_Position = uViewProjectionMatrix[face] * gPosition;
            EmitVertex();
        }    
        EndPrimitive();
    }
}  