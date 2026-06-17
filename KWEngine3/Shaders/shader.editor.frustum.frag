#version 400 core

uniform vec3 uColorTint;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

void main()
{
    color = vec4(uColorTint, 1.0);
    bloom = vec4(0.0, 0.0, 0.0, 1.0);
}