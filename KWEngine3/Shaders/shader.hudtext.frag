#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;
uniform vec4 uCursorInfo;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
    float globalVisibility = 1.0;
    float tex = texture(uTexture, vTexture).x;

    if(uCursorInfo.x != 0)
    {
        // blink = -1
        // fade  = +1
        float sinV = sin(uCursorInfo.y * uCursorInfo.z * 10.0) * 0.5 + 0.5;
        globalVisibility *= uCursorInfo.x < 0 ? step(0.5, sinV) : sinV;
    }

    color = tex * uColorTint * uColorTint.w * globalVisibility;
    bloom.x = uColorGlow.x * uColorGlow.w;
    bloom.y = uColorGlow.y * uColorGlow.w;
    bloom.z = uColorGlow.z * uColorGlow.w;
    bloom.w = uColorTint.w * tex * globalVisibility;
}