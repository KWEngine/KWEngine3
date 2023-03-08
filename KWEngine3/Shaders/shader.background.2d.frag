#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec3 uColorAmbient;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
    color = texture(uTexture, vTexture) * vec4(uColorAmbient, 1.0);
    color.w = 1.0;
    bloom = vec4(0);
}