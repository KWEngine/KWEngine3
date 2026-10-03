#version 400 core

in vec2 vTexture;

// target: FramebufferLightingPass, Blending (SrcAlpha, OneMinusSrcAlpha)
layout(location = 0) out vec4 color; // color:  scene * (1 - a) + fogColor * a
layout(location = 1) out vec4 bloom; // bloom:  bloom * (1 - a): brighter objects still do not shine through the fog

uniform sampler2D uTextureDepth;              // depth buffer
uniform mat4 uViewProjectionMatrixInverted;
uniform vec3 uCameraPos;

#include "fog.glsl"

vec3 getWorldPosition()
{
    float depth = texture(uTextureDepth, vTexture).r * 2.0 - 1.0;
    vec4 worldSpaceCoordinate = uViewProjectionMatrixInverted * vec4(vTexture * 2.0 - 1.0, depth, 1.0);
    return worldSpaceCoordinate.xyz / worldSpaceCoordinate.w;
}

void main()
{
    vec4 fog = fogIntegrate(uCameraPos, getWorldPosition());
    float a = 1.0 - fog.w;

    // blending: fog.xyz + scene * transmittance
    color = vec4(a > 0.00001 ? fog.xyz / a : vec3(0.0), a);
    bloom = vec4(0.0, 0.0, 0.0, a);
}
