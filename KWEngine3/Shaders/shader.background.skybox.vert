#version 400 core
 
layout(location = 0) in	vec3 aPosition;

out		vec3 vTexture;

uniform mat4 uViewProjectionMatrix;
 
void main()
{
    vTexture = aPosition; 
    gl_Position = (uViewProjectionMatrix * vec4(aPosition, 1.0)).xyww;
}