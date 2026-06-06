#version 400 core

layout(points) in;
layout(line_strip, max_vertices = 24) out;

out float gOpacity;

uniform vec2 uRadius;
uniform mat4 uViewProjectionMatrix;

const float f = 0.975;

float GetOpacity(vec4 offset)
{
    float bottomY = -uRadius.y;
    float topY = uRadius.y;

    float t = (offset.y - bottomY) / (topY - bottomY);

    t = clamp(t, 0.0, 1.0);

    return mix(0.75, 0.0, t);
}

void EmitVertexWithOpacity(vec4 offset)
{
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + offset);
    gOpacity = GetOpacity(offset);
    EmitVertex();
}

void EmitLine(vec4 a, vec4 b)
{
    EmitVertexWithOpacity(a);
    EmitVertexWithOpacity(b);
    EndPrimitive();
}

void main()
{
    float rx = uRadius.x * f;
    float ry = uRadius.y;
    float rz = uRadius.x * f;

    vec4 frontTopRight    = vec4( rx,  ry,  rz, 0.0);
    vec4 frontTopLeft     = vec4(-rx,  ry,  rz, 0.0);
    vec4 frontBottomLeft  = vec4(-rx, -ry,  rz, 0.0);
    vec4 frontBottomRight = vec4( rx, -ry,  rz, 0.0);

    vec4 backTopRight     = vec4( rx,  ry, -rz, 0.0);
    vec4 backTopLeft      = vec4(-rx,  ry, -rz, 0.0);
    vec4 backBottomLeft   = vec4(-rx, -ry, -rz, 0.0);
    vec4 backBottomRight  = vec4( rx, -ry, -rz, 0.0);

    // front
    EmitLine(frontTopRight, frontTopLeft);
    EmitLine(frontTopLeft, frontBottomLeft);
    EmitLine(frontBottomLeft, frontBottomRight);
    EmitLine(frontBottomRight, frontTopRight);

    // back
    EmitLine(backTopRight, backTopLeft);
    EmitLine(backTopLeft, backBottomLeft);
    EmitLine(backBottomLeft, backBottomRight);
    EmitLine(backBottomRight, backTopRight);

    // connections
    EmitLine(frontTopLeft, backTopLeft);
    EmitLine(frontTopRight, backTopRight);
    EmitLine(frontBottomLeft, backBottomLeft);
    EmitLine(frontBottomRight, backBottomRight);
}