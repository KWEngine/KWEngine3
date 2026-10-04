#version 400 core

in		vec2 vTexture;

uniform sampler2D uTexture;
uniform float uBloomRadius;
uniform vec3 uBoxParams; // x, y: tap offset in source texels, z: energy factor (z = 0: regular 9-tap downsample)

out		vec4 color;

// note: all bloom textures have no mipmaps -> textureLod(..., 0.0) gives the same
// result as texture() but skips the LOD calculation (sample_lz on Intel GPUs)

vec4 DownsampleBox13Tap(sampler2D samplerTex, vec2 texelSize, float bloomRadius)
{
    vec4 A = textureLod(samplerTex, vTexture + texelSize * vec2(-1.0, -1.0)* bloomRadius, 0.0);
    vec4 B = textureLod(samplerTex, vTexture + texelSize * vec2( 0.0, -1.0)* bloomRadius, 0.0);
    vec4 C = textureLod(samplerTex, vTexture + texelSize * vec2( 1.0, -1.0)* bloomRadius, 0.0);
    vec4 D = textureLod(samplerTex, vTexture + texelSize * vec2(-0.5, -0.5)* bloomRadius, 0.0);
    vec4 E = textureLod(samplerTex, vTexture + texelSize * vec2( 0.5, -0.5)* bloomRadius, 0.0);
    vec4 F = textureLod(samplerTex, vTexture + texelSize * vec2(-1.0,  0.0)* bloomRadius, 0.0);
    vec4 G = textureLod(samplerTex, vTexture                                            , 0.0);
    vec4 H = textureLod(samplerTex, vTexture + texelSize * vec2( 1.0,  0.0)* bloomRadius, 0.0);
    vec4 I = textureLod(samplerTex, vTexture + texelSize * vec2(-0.5,  0.5)* bloomRadius, 0.0);
    vec4 J = textureLod(samplerTex, vTexture + texelSize * vec2( 0.5,  0.5)* bloomRadius, 0.0);
    vec4 K = textureLod(samplerTex, vTexture + texelSize * vec2(-1.0,  1.0)* bloomRadius, 0.0);
    vec4 L = textureLod(samplerTex, vTexture + texelSize * vec2( 0.0,  1.0)* bloomRadius, 0.0);
    vec4 M = textureLod(samplerTex, vTexture + texelSize * vec2( 1.0,  1.0)* bloomRadius, 0.0);

    vec2 div = (1.0 / 4.0) * vec2(0.5, 0.125);

    vec4 o = (D + E + I + J) * div.x;
    o += (A + B + G + F) * div.y;
    o += (B + C + H + G) * div.y;
    o += (F + G + L + K) * div.y;
    o += (G + H + M + L) * div.y;

    return o;
}

vec4 DownsampleBoxLQTap(sampler2D samplerTex, vec2 texelSize, float bloomRadius)
{
    vec4 B = textureLod(samplerTex, vTexture + texelSize * vec2( 0.0, -1.0)* bloomRadius, 0.0);
    vec4 D = textureLod(samplerTex, vTexture + texelSize * vec2(-0.5, -0.5)* bloomRadius, 0.0);
    vec4 E = textureLod(samplerTex, vTexture + texelSize * vec2( 0.5, -0.5)* bloomRadius, 0.0);
    vec4 F = textureLod(samplerTex, vTexture + texelSize * vec2(-1.0,  0.0)* bloomRadius, 0.0);
    vec4 G = textureLod(samplerTex, vTexture                                            , 0.0);
    vec4 H = textureLod(samplerTex, vTexture + texelSize * vec2( 1.0,  0.0)* bloomRadius, 0.0);
    vec4 I = textureLod(samplerTex, vTexture + texelSize * vec2(-0.5,  0.5)* bloomRadius, 0.0);
    vec4 J = textureLod(samplerTex, vTexture + texelSize * vec2( 0.5,  0.5)* bloomRadius, 0.0);
    vec4 L = textureLod(samplerTex, vTexture + texelSize * vec2( 0.0,  1.0)* bloomRadius, 0.0);

    vec2 div = (1.0 / 4.0) * vec2(0.5, 0.125);

    vec4 o = (D + E + I + J) * div.x;
    o += (B + G + F) * div.y;
    o += (B + H + G) * div.y;
    o += (F + G + L) * div.y;
    o += (G + H + L) * div.y;

    return o;
}

// first pass of the half resolution chain (Default/Low): box filter over about
// (ratio x ratio) source texels with 4 bilinear taps (needs a linear sampler)
vec4 DownsampleBox4Tap(sampler2D samplerTex, vec2 texelSize)
{
    vec2 d = texelSize * uBoxParams.xy;
    vec4 s = textureLod(samplerTex, vTexture + vec2(-d.x, -d.y), 0.0);
    s += textureLod(samplerTex, vTexture + vec2( d.x, -d.y), 0.0);
    s += textureLod(samplerTex, vTexture + vec2(-d.x,  d.y), 0.0);
    s += textureLod(samplerTex, vTexture + vec2( d.x,  d.y), 0.0);
    return s * (0.25 * uBoxParams.z);
}

void main()
{
    vec2 texelSize = 1.0 / textureSize(uTexture, 0);
    if(uBoxParams.z > 0.0)
        color = DownsampleBox4Tap(uTexture, texelSize);
    else
        color = DownsampleBoxLQTap(uTexture, texelSize, uBloomRadius * 1.25);
}