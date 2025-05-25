#version 400 core

in vec4 gPosition;

uniform vec2 uNearFar;
uniform vec3 uLightPosition;

void main()
{
    gl_FragDepth = length(gPosition.xyz - uLightPosition) / uNearFar.y;
}  