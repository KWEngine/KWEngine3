#version 400 core

in		vec4 vPosition;

uniform vec4 uColorEmissive;
uniform vec3 uColorAmbient;

layout(location = 0) out vec3 albedo; //R11G11B10f
layout(location = 1) out vec2 normal; //rg8ui
layout(location = 2) out vec3 metallicRoughnessMetallicType; // rgb8
layout(location = 3) out vec3 idShadowCaster; // rgb8

vec2 octWrap(vec2 v)
{
    //return (1.0 - abs(v.yx)) * (v.xy >= 0.0 ? 1.0 : -1.0);
    return (1.0 - abs(v.yx)) * sign(v.xy);
}
 
vec2 encodeNormal(vec3 n)
{
    n /= (abs(n.x) + abs(n.y) + abs(n.z));
    n.xy = n.z >= 0.0 ? n.xy : octWrap(n.xy);
    n.xy = n.xy * 0.5 + vec2(0.5);
    return n.xy;
}

void main()
{
	albedo = vec3(uColorAmbient + uColorEmissive.xyz * uColorEmissive.w);
	normal = encodeNormal(vec3(0.0, 0.0, 0.0));
	idShadowCaster = vec3(1.0, 1.0, 1 / 255.0); 
	metallicRoughnessMetallicType = vec3(0.0, 1.0, 0.0);
}