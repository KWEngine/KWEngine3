#version 400 core

in		vec2 vTexture;

uniform sampler2D uTextureSmaller;  // smaller tex (bloom)
uniform sampler2D uTextureBigger;  // bigger tex (fb)
uniform vec3 uBloomRadius;

out		vec4 color;

vec4 sampleTent(sampler2D tex, vec2 texelSize, float scale)
{
    vec4 d = texelSize.xyxy * vec4(1.0, 1.0, -1.0, 0.0) * scale;
    vec4 s = vec4(0.0);
    s =  texture(tex, vTexture - d.xy);
    s += texture(tex, vTexture - d.wy) * 2.0;
    s += texture(tex, vTexture - d.zy);

    s += texture(tex, vTexture + d.zw) * 2.0;
    s += texture(tex, vTexture       ) * 4.0;
    s += texture(tex, vTexture + d.xw) * 2.0;

    s += texture(tex, vTexture + d.zy);
    s += texture(tex, vTexture + d.wy) * 2.0;
    s += texture(tex, vTexture + d.xy);

    return s * (1.0 / 16.0);
}

vec4 sampleSimple(sampler2D tex, vec2 texelSize, float scale)
{
    return texture(tex, vTexture);
}

vec4 combine(vec4 tex1, vec4 tex2)
{
    return vec4(tex1.rgb + tex2.rgb, 1.0);
}

vec4 sampleBox(sampler2D tex, vec2 texelSize, float scale)
{
    vec4 d = texelSize.xyxy * vec4(-1.0, -1.0, 1.0, 1.0) * (scale * 0.5);

    vec4 s;
    s =  texture(tex, vTexture + d.xy);
    s += texture(tex, vTexture + d.zy);
    s += texture(tex, vTexture + d.xw);
    s += texture(tex, vTexture + d.zw);
    return s * (1.0 / 4.0);
}

void main()
{
    vec2 txSmall = 1.0 / textureSize(uTextureSmaller, 0);
    vec2 txBig = 1.0 / textureSize(uTextureBigger, 0);

    vec4 colorSmallerTex = sampleTent(uTextureSmaller, txSmall, uBloomRadius.x) * (uBloomRadius.y + uBloomRadius.x); 
    vec4 colorBiggerTex = sampleTent(uTextureBigger, txBig, uBloomRadius.x) * (uBloomRadius.z + (1.0 - uBloomRadius.x));

    color = combine(colorSmallerTex, colorBiggerTex);
}
