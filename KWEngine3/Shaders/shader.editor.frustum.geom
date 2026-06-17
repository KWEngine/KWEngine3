#version 400 core

layout(points) in;
layout(line_strip, max_vertices = 24) out;

uniform mat4 uVPEditor;
uniform mat4 uVPGame;

void EmitLine(vec3 a, vec3 b)
{
    gl_Position = uVPEditor * vec4(a, 1.0);
    EmitVertex();

    gl_Position = uVPEditor * vec4(b, 1.0);
    EmitVertex();

    EndPrimitive();
}

vec3 NdcToWorld(vec3 ndc, mat4 invVP)
{
    vec4 world = invVP * vec4(ndc, 1.0);
    return world.xyz / world.w;
}

void main()
{
    mat4 invVPGame = inverse(uVPGame);

    // OpenGL-NDC:
    // x: -1 left, +1 right
    // y: -1 bottom, +1 top
    // z: -1 near, +1 far

    vec3 nbl = NdcToWorld(vec3(-1.0, -1.0, -1.0), invVPGame); // near bottom left
    vec3 nbr = NdcToWorld(vec3( 1.0, -1.0, -1.0), invVPGame); // near bottom right
    vec3 ntr = NdcToWorld(vec3( 1.0,  1.0, -1.0), invVPGame); // near top right
    vec3 ntl = NdcToWorld(vec3(-1.0,  1.0, -1.0), invVPGame); // near top left

    vec3 fbl = NdcToWorld(vec3(-1.0, -1.0,  1.0), invVPGame); // far bottom left
    vec3 fbr = NdcToWorld(vec3( 1.0, -1.0,  1.0), invVPGame); // far bottom right
    vec3 ftr = NdcToWorld(vec3( 1.0,  1.0,  1.0), invVPGame); // far top right
    vec3 ftl = NdcToWorld(vec3(-1.0,  1.0,  1.0), invVPGame); // far top left

    // Near-Plane
    EmitLine(nbl, nbr);
    EmitLine(nbr, ntr);
    EmitLine(ntr, ntl);
    EmitLine(ntl, nbl);

    // Far-Plane
    EmitLine(fbl, fbr);
    EmitLine(fbr, ftr);
    EmitLine(ftr, ftl);
    EmitLine(ftl, fbl);

    // lines between near und far
    EmitLine(nbl, fbl);
    EmitLine(nbr, fbr);
    EmitLine(ntr, ftr);
    EmitLine(ntl, ftl);
}