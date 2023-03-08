#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
    vec4 tex = texture(uTexture, vTexture);
    
    color = tex * uColorTint * uColorTint.w;
    bloom.x = uColorGlow.x * uColorGlow.w;
    bloom.y = uColorGlow.y * uColorGlow.w;
    bloom.z = uColorGlow.z * uColorGlow.w;
    bloom.w = uColorGlow.w * tex.w;
    
}