#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;
uniform vec4 uCursorInfo;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 

float median(float r, float g, float b) 
{
    return max(min(r, g), min(max(r, g), b));
}

float median(vec3 rgb)
{
    return max(min(rgb.r, rgb.g), min(max(rgb.r, rgb.g), rgb.b));
}

float pxRange = 2.0;

void main()
{
    float outlineWidth = 0.0;
    vec3 outlineColor = vec3(1, 0, 0);

    float globalVisibility = 1.0;
    vec3 tex = texture(uTexture, vTexture).rgb;
    float sd = median(tex.r, tex.g, tex.b);

    float screenPxDistance = pxRange * (sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);

    float outlineFactor = clamp(screenPxDistance + 0.5 + outlineWidth, 0.0, 1.0);
    if(outlineFactor < 0.0001)
    {
        discard;
    }

    vec3 finalColor = mix(outlineColor, uColorTint.rgb, opacity);

    if(uCursorInfo.x != 0)
    {
        // blink = -1
        // fade  = +1
        float sinV = sin(uCursorInfo.y * uCursorInfo.z * 10.0) * 0.5 + 0.5;
        globalVisibility *= uCursorInfo.x < 0 ? step(0.5, sinV) : sinV;
    }

    color = vec4(finalColor, uColorTint.w * globalVisibility);
    bloom.x = uColorGlow.x * uColorGlow.w;
    bloom.y = uColorGlow.y * uColorGlow.w;
    bloom.z = uColorGlow.z * uColorGlow.w;
    bloom.w = uColorTint.w * opacity * globalVisibility;
}