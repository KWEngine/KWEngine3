#version 400 core
 
in		vec2 vTexture;
in      vec4 vNDC;

uniform sampler2D uTexture;
uniform vec4 uColorTint;
uniform vec4 uColorGlow;
uniform int uOptions;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
    float globalVisibility = 1.0;
    if(uOptions > 0)
    {
        vec2 ndc = vNDC.xy / vNDC.w;
        float len = length(ndc - vec2(0.0));
        globalVisibility = 1.0 - smoothstep(0.975, 1.025, len);
    }
    vec4 tex = texture(uTexture, vTexture);

    bloom = vec4(0,0,0,1);
    color = tex * uColorTint * uColorTint.w * globalVisibility;
    bloom.x = uColorGlow.x * uColorGlow.w;
    bloom.y = uColorGlow.y * uColorGlow.w;
    bloom.z = uColorGlow.z * uColorGlow.w;
    bloom.w = uColorGlow.w * tex.w * globalVisibility;
}