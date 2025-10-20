#version 400 core

in vec2 vTexture;

layout(location = 0) out float shade;

uniform sampler2D uTextureSSAO;

const int start = -2;
const int end = 3;
const int delta = end - start;
const int deltaSq = delta * delta;

void main()
{
    

    vec2 texelSize = 1.0 / vec2(textureSize(uTextureSSAO, 0));
    float result = 0.0;
    for (int x = start; x < end; x++) 
    {
        for (int y = start; y < end; y++) 
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            result += texture(uTextureSSAO, vTexture + offset).r;
        }
    }
    shade = result / deltaSq;
}