#version 400 core

in		vec2 vTexture;

uniform sampler2D uTexture;
uniform float uBloomRadius;

out		vec4 color;

vec4 DownsampleBox13Tap(sampler2D samplerTex, vec2 texelSize, float bloomRadius)
{
    vec4 A = texture(samplerTex, vTexture + texelSize * vec2(-1.0, -1.0)* bloomRadius);
    vec4 B = texture(samplerTex, vTexture + texelSize * vec2( 0.0, -1.0)* bloomRadius);
    vec4 C = texture(samplerTex, vTexture + texelSize * vec2( 1.0, -1.0)* bloomRadius);
    vec4 D = texture(samplerTex, vTexture + texelSize * vec2(-0.5, -0.5)* bloomRadius);
    vec4 E = texture(samplerTex, vTexture + texelSize * vec2( 0.5, -0.5)* bloomRadius);
    vec4 F = texture(samplerTex, vTexture + texelSize * vec2(-1.0,  0.0)* bloomRadius);
    vec4 G = texture(samplerTex, vTexture                                            );
    vec4 H = texture(samplerTex, vTexture + texelSize * vec2( 1.0,  0.0)* bloomRadius);
    vec4 I = texture(samplerTex, vTexture + texelSize * vec2(-0.5,  0.5)* bloomRadius);
    vec4 J = texture(samplerTex, vTexture + texelSize * vec2( 0.5,  0.5)* bloomRadius);
    vec4 K = texture(samplerTex, vTexture + texelSize * vec2(-1.0,  1.0)* bloomRadius);
    vec4 L = texture(samplerTex, vTexture + texelSize * vec2( 0.0,  1.0)* bloomRadius);
    vec4 M = texture(samplerTex, vTexture + texelSize * vec2( 1.0,  1.0)* bloomRadius);

    vec2 div = (1.0 / 4.0) * vec2(0.5, 0.125);

    vec4 o = (D + E + I + J) * div.x;
    o += (A + B + G + F) * div.y;
    o += (B + C + H + G) * div.y;
    o += (F + G + L + K) * div.y;
    o += (G + H + M + L) * div.y;

    return o;
}

void main()
{
    vec2 texelSize = 1.0 / textureSize(uTexture, 0);
    color = DownsampleBox13Tap(uTexture, texelSize, uBloomRadius * 1.25);
}