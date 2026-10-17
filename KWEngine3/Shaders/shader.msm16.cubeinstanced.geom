#version 400 core
layout (triangles) in;
layout (triangle_strip, max_vertices=18) out;

in vec2 vTexture[];

uniform mat4 uViewProjectionMatrix[6];

out vec4 gPosition; // gPosition from GS (output per emitvertex)
out vec2 gTexture;

// true if all three vertices lie outside of the same clip plane (then the triangle cannot be visible)
bool isTriangleOutside(vec4 a, vec4 b, vec4 c)
{
    return (a.x >  a.w && b.x >  b.w && c.x >  c.w) ||
           (a.x < -a.w && b.x < -b.w && c.x < -c.w) ||
           (a.y >  a.w && b.y >  b.w && c.y >  c.w) ||
           (a.y < -a.w && b.y < -b.w && c.y < -c.w) ||
           (a.z >  a.w && b.z >  b.w && c.z >  c.w) ||
           (a.z < -a.w && b.z < -b.w && c.z < -c.w);
}

void main()
{
    for(int face = 0; face < 6; ++face)
    {
        vec4 p0 = uViewProjectionMatrix[face] * gl_in[0].gl_Position;
        vec4 p1 = uViewProjectionMatrix[face] * gl_in[1].gl_Position;
        vec4 p2 = uViewProjectionMatrix[face] * gl_in[2].gl_Position;
        if(isTriangleOutside(p0, p1, p2))
        {
            continue;
        }
        vec4 p[3] = vec4[3](p0, p1, p2);

        gl_Layer = face; // built-in variable that specifies to which face we render.
        for(int i = 0; i < 3; ++i) // for each triangle vertex
        {
            gPosition = gl_in[i].gl_Position;
            gTexture = vTexture[i];
            gl_Position = p[i];
            EmitVertex();
        }    
        EndPrimitive();
    }
}  