#version 400 core
 
layout(location = 0) in	vec3 aPosition;
layout(location = 1) in vec2 aTexture;

out		vec3 vTexture;
out     vec2 vTextureEquirectangular;

uniform mat4 uViewProjectionMatrix;
 
void main()
{
    vTexture = aPosition; 
    vTextureEquirectangular = aTexture;
    gl_Position = (uViewProjectionMatrix * vec4(aPosition, 1.0)).xyww;
}