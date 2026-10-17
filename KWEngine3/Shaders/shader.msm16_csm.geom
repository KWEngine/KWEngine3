#version 400 core

layout(triangles, invocations = 2) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 uViewProjectionMatrix[2];

in		vec2  vTexture[];

out		float gZ;
out		float gW;
out		vec2  gTexture;

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
    vec4 p0 = uViewProjectionMatrix[gl_InvocationID] * gl_in[0].gl_Position;
    vec4 p1 = uViewProjectionMatrix[gl_InvocationID] * gl_in[1].gl_Position;
    vec4 p2 = uViewProjectionMatrix[gl_InvocationID] * gl_in[2].gl_Position;
    if(isTriangleOutside(p0, p1, p2))
    {
        return;
    }
    vec4 p[3] = vec4[3](p0, p1, p2);

	for (int i = 0; i < 3; i++)
    {
        vec4 transformedPosition = p[i];
	    gZ = transformedPosition.z;
	    gW = transformedPosition.w;

        gl_Position = transformedPosition;
        gl_Layer = gl_InvocationID;    
        gTexture = vTexture[i];
        EmitVertex();
    }
    EndPrimitive();
}