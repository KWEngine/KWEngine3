#version 400 core
 
in		vec2 vTexture;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;
uniform vec4 uCursorInfo;
uniform vec4 uColorOutline; // w = outlinewidth
uniform float uScale;

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

void main()
{
    
// ---- Parameter ----
    const float PxRangeAtlas = 8.0; 
    float SoftnessPxFill = uScale < 32 ? 3.5 : 6.5;
    float SoftnessPxOutline = uScale < 32 ? 5.5 : 4.5;

    vec3 tex = texture(uTexture, vTexture).rgb;
    float sd_msdf = median(tex);

    float sdw = fwidth(sd_msdf);
    float inv = PxRangeAtlas / max(sdw, 1e-6);

    float pxDist_msdf = (sd_msdf - 0.5) * inv;
    float sd_sdf = sd_msdf;
    float pxDist_sdf = (sd_sdf - 0.5) * inv;

    float stepA = uScale < 32 ? 0.06 : 0.03;
    float stepB = uScale < 32 ? 0.00 : 0.01;
    float sdfBlend = smoothstep(stepA, stepB, sdw);  // 0 = MSDF, 1 = SDF (0.03, 0.01)
    float pxDist = mix(pxDist_msdf, pxDist_sdf, sdfBlend);

    float opacity = smoothstep(-SoftnessPxFill, +SoftnessPxFill, pxDist);

    float outlineShiftAtlas = clamp(uColorOutline.w, 0.0, 0.49);
    float pxDistOutline = ((sd_msdf - 0.5) + outlineShiftAtlas) * inv;

    float pxDistOutlineMix = mix(pxDistOutline,
                                    ((sd_sdf - 0.5) + outlineShiftAtlas) * inv,
                                    sdfBlend
                                    );

    float outlineFactor = smoothstep(-SoftnessPxOutline, +SoftnessPxOutline, pxDistOutlineMix);

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