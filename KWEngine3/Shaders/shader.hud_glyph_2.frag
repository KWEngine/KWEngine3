#version 400

layout(location = 0) out float r8;
layout(location = 1) out vec4 color;

in vec3 vBarycentric;

uniform vec4 uColorTint;

void main()
{
    float r = vBarycentric.x;
    float s = vBarycentric.y;
    float t = vBarycentric.z;

    float result = pow(s * 0.5 + t, 2);
    if(result < t)
    {
        r8 = 1.0 / 255.0;
        color = uColorTint;
    }
    else
    {
        r8 = 0.0;
        color = vec4(0.0);
    }   
}