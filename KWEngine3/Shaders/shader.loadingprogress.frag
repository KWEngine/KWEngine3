#version 400 core

in vec2 vTexture;

uniform int   uType;             // 0 = Bar, 1 = FilledCircle (Pie), 2 = UnfilledCircle (Ring)
uniform float uProgress;         // 0..1
uniform vec4  uColor;            // rgb + alpha
uniform float uBorderThickness;  // Bar: fraction of shorter side; Circles: fraction of radius
uniform vec2  uSize;             // pixel size of the element (width, height)

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

const float PI2 = 6.28318530718;

void main()
{
    vec2 uv = vTexture;
    float alpha = 0.0;
    bool isBorder = false;

    // -------------------------------------------------------------------------
    // Bar
    // -------------------------------------------------------------------------
    if (uType == 0)
    {
        // Pixel-accurate border width: fraction of the shorter side / 2
        float borderPx = uBorderThickness * min(uSize.x, uSize.y) * 0.5;
        float bx = borderPx / uSize.x;
        float by = borderPx / uSize.y;

        bool inBorder = (uv.x < bx || uv.x > 1.0 - bx ||
                         uv.y < by || uv.y > 1.0 - by);

        // Inner fill area: margin = one border-width gap inward
        float mx = 2.0 * bx;
        float my = 2.0 * by;
        bool inInner = (uv.x >= mx && uv.x <= 1.0 - mx &&
                        uv.y >= my && uv.y <= 1.0 - my);

        float innerWidth = 1.0 - 2.0 * mx;
        float innerProgress = (innerWidth > 0.0) ? (uv.x - mx) / innerWidth : 0.0;
        bool filled = inInner && (innerProgress <= uProgress);

        if (inBorder && uBorderThickness > 0.0)
        {
            alpha = 1.0;
            isBorder = true;
        }
        else if (filled)
        {
            alpha = 1.0;
        }
    }
    // -------------------------------------------------------------------------
    // FilledCircle (Pie)
    // -------------------------------------------------------------------------
    else if (uType == 1)
    {
        vec2 p = uv * 2.0 - 1.0; // [-1, 1]
        float dist = length(p);

        if (dist <= 1.0)
        {
            // Angle: 0 at top, clockwise -> negate p.y because screen-y increases downward
            float angle = atan(p.x, -p.y);   // -pi .. pi
            if (angle < 0.0) angle += PI2;   // 0 .. 2pi

            if (angle <= uProgress * PI2)
            {
                alpha = 1.0;
            }

            // Outer border ring
            if (uBorderThickness > 0.0 && dist > 1.0 - uBorderThickness)
            {
                alpha = 1.0;
                isBorder = true;
            }
        }
    }
    // -------------------------------------------------------------------------
    // UnfilledCircle (Ring-Arc)
    // -------------------------------------------------------------------------
    else if (uType == 2)
    {
        vec2 p = uv * 2.0 - 1.0;
        float dist = length(p);
        float innerRadius = max(0.0, 1.0 - uBorderThickness);

        if (dist <= 1.0 && dist >= innerRadius)
        {
            float angle = atan(p.x, -p.y);
            if (angle < 0.0) angle += PI2;

            if (angle <= uProgress * PI2)
            {
                alpha = 1.0;
            }
        }
    }

    if (alpha <= 0.0) discard;

    // Color computation
    vec3 finalRGB = uColor.rgb;

    color = vec4(finalRGB * uColor.a, alpha * uColor.a);

    // Write to bloom only when finalRGB values exceed 1.0
    bloom = vec4(
        max(0.0, color.x - 1.0),
        max(0.0, color.y - 1.0),
        max(0.0, color.z - 1.0),
        1.0);
    
}