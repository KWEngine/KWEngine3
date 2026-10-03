#version 400 core

in vec2 vTexture;

layout(location = 0) out float shade;

uniform sampler2D uTextureSSAO; // needs linear filtering

// 4x4 box filter (same size as the 4x4 noise tile) built from 4 bilinear taps.
void main()
{
    vec2 texelSize = 1.0 / vec2(textureSize(uTextureSSAO, 0));
    float result = texture(uTextureSSAO, vTexture + vec2(-1.5, -1.5) * texelSize).r;
    result      += texture(uTextureSSAO, vTexture + vec2( 0.5, -1.5) * texelSize).r;
    result      += texture(uTextureSSAO, vTexture + vec2(-1.5,  0.5) * texelSize).r;
    result      += texture(uTextureSSAO, vTexture + vec2( 0.5,  0.5) * texelSize).r;
    shade = result * 0.25;
}
