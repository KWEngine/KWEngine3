#version 400 core
 
in      vec4 vNDC;
uniform int uOptions;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;
 
void main()
{
    float globalVisibility = 0.0;
    if(uOptions > 0)
    {
        vec2 ndc = vNDC.xy / vNDC.w;
        float len = length(ndc - vec2(0.0));
        globalVisibility = 1.0 - clamp(len, 0.0, 1.0);
    }

    color = vec4(0,0,0,globalVisibility);
    bloom = vec4(0,0,0,1);
}