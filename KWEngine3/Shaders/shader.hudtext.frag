#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;
uniform vec4 uCursorInfo;
uniform vec4 uColorOutline; // w = outlinewidth

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

float pxRange = 6.0;

void main()
{
    vec3 tex = texture(uTexture, vTexture).rgb;
    float sd = median(tex);
    float sdw = fwidth(sd);

    float screenPxDistance = (sd - 0.5) * pxRange / max(sdw, 1e-6);
    float opacity = smoothstep(-4.5, +4.5, screenPxDistance);

    
    float outlinePxDistance = ((sd - 0.5) + clamp(uColorOutline.w, 0.0, 0.49)) * pxRange / fwidth(sd);
    float outlineFactor =  smoothstep(-2.5, +2.5, outlinePxDistance);


    if(outlineFactor < 0.00001) discard;

    vec3 finalColor = mix(uColorOutline.rgb, uColorTint.rgb, opacity);
    
    float globalVisibility = 1.0;
    if(uCursorInfo.x != 0)
    {
        // blink = -1
        // fade  = +1
        float sinV = sin(uCursorInfo.y * uCursorInfo.z * 10.0) * 0.5 + 0.5;
        globalVisibility *= uCursorInfo.x < 0 ? step(0.5, sinV) : sinV;
    }
    float finalAlpha = outlineFactor * uColorTint.w * globalVisibility;

    color = vec4(finalColor, finalAlpha);

    if(color.x > 1.0)
        bloom.x = color.x - 1.0;
    if(color.y > 1.0)
        bloom.y = color.y - 1.0;
    if(color.z > 1.0)
        bloom.z = color.z - 1.0;

    bloom.x += uColorGlow.x * uColorGlow.w;
    bloom.y += uColorGlow.y * uColorGlow.w;
    bloom.z += uColorGlow.z * uColorGlow.w;
    bloom.w += uColorTint.w * globalVisibility;
}