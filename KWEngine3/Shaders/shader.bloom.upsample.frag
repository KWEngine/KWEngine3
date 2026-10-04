#version 400 core

in		vec2 vTexture;

uniform sampler2D uTextureSmaller;  // smaller tex (bloom)
uniform sampler2D uTextureBigger;  // bigger tex (fb)
uniform vec3 uBloomParams; // x: tap distance in texels, y: weight of the smaller texture, z: weight of the bigger texture

out		vec4 color;

vec3 sampleTent9(sampler2D tex, vec2 texelSize, float scale)
{
    vec4 d = texelSize.xyxy * vec4(1.0, 1.0, -1.0, 0.0) * scale;
    vec3 s;
    s =  textureLod(tex, vTexture - d.xy, 0.0).rgb;
    s += textureLod(tex, vTexture - d.wy, 0.0).rgb * 2.0;
    s += textureLod(tex, vTexture - d.zy, 0.0).rgb;

    s += textureLod(tex, vTexture + d.zw, 0.0).rgb * 2.0;
    s += textureLod(tex, vTexture       , 0.0).rgb * 4.0;
    s += textureLod(tex, vTexture + d.xw, 0.0).rgb * 2.0;

    s += textureLod(tex, vTexture + d.zy, 0.0).rgb;
    s += textureLod(tex, vTexture + d.wy, 0.0).rgb * 2.0;
    s += textureLod(tex, vTexture + d.xy, 0.0).rgb;

    return s * (1.0 / 16.0);
}

vec3 sampleTent4Aligned(sampler2D tex, vec2 texelSize, float scale)
{
    vec2 d = texelSize * (0.5 * scale);
    vec3 s;
    s =  textureLod(tex, vTexture + vec2(-d.x, -d.y), 0.0).rgb;
    s += textureLod(tex, vTexture + vec2( d.x, -d.y), 0.0).rgb;
    s += textureLod(tex, vTexture + vec2(-d.x,  d.y), 0.0).rgb;
    s += textureLod(tex, vTexture + vec2( d.x,  d.y), 0.0).rgb;
    return s * 0.25;
}

// Tent weights of the texels c-1, c, c+1, c+2 for sub-texel offset f in [0;1)
// and tap distance scale <= 1 (sum of three linear interpolation hats).
vec4 tentWeights(float f, float scale)
{
    vec4 k = vec4(-1.0, 0.0, 1.0, 2.0);
    return 0.25 * max(1.0 - abs(f - scale - k), 0.0)
         + 0.50 * max(1.0 - abs(f         - k), 0.0)
         + 0.25 * max(1.0 - abs(f + scale - k), 0.0);
}

vec3 sampleTent4(sampler2D tex, vec2 texSize, vec2 texelSize, float scale)
{
    vec2 p = vTexture * texSize - 0.5;
    vec2 c = floor(p);
    vec2 f = p - c;
    vec4 wx = tentWeights(f.x, scale);
    vec4 wy = tentWeights(f.y, scale);

    vec2 wL = vec2(wx.x + wx.y, wy.x + wy.y);
    vec2 wR = vec2(wx.z + wx.w, wy.z + wy.w);
    vec2 uvL = (c - 0.5 + vec2(wx.y, wy.y) / wL) * texelSize;
    vec2 uvR = (c + 1.5 + vec2(wx.w, wy.w) / max(wR, vec2(1.0e-6))) * texelSize;

    vec3 s;
    s =  textureLod(tex, uvL, 0.0).rgb * (wL.x * wL.y);
    s += textureLod(tex, vec2(uvR.x, uvL.y), 0.0).rgb * (wR.x * wL.y);
    s += textureLod(tex, vec2(uvL.x, uvR.y), 0.0).rgb * (wL.x * wR.y);
    s += textureLod(tex, uvR, 0.0).rgb * (wR.x * wR.y);
    return s;
}

vec4 sampleSimple(sampler2D tex, vec2 texelSize, float scale)
{
    return texture(tex, vTexture);
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
    ivec2 sizeSmall = textureSize(uTextureSmaller, 0);
    vec2 txSmall = 1.0 / vec2(sizeSmall);
    vec2 txBig = 1.0 / vec2(textureSize(uTextureBigger, 0));

    vec3 colorSmallerTex;
    vec3 colorBiggerTex;
    if(uBloomParams.x <= 1.0)
    {
        // exact for scale <= 1: 8 instead of 18 texture fetches
        colorSmallerTex = sampleTent4(uTextureSmaller, vec2(sizeSmall), txSmall, uBloomParams.x);
        colorBiggerTex = sampleTent4Aligned(uTextureBigger, txBig, uBloomParams.x);
    }
    else
    {
        colorSmallerTex = sampleTent9(uTextureSmaller, txSmall, uBloomParams.x);
        colorBiggerTex = sampleTent9(uTextureBigger, txBig, uBloomParams.x);
    }

    color = vec4(colorSmallerTex * uBloomParams.y + colorBiggerTex * uBloomParams.z, 1.0);
}