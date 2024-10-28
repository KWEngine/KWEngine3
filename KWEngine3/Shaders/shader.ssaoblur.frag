#version 400 core

in vec2 vTexture;

layout(location = 0) out float shade;

uniform sampler2D uTextureSSAO;

void main()
{
    vec2 texelSize = 1.0 / vec2(textureSize(uTextureSSAO, 0));
    float result = 0.0;
    for (int x = -1; x < 2; x++) 
    {
        for (int y = -1; y < 2; y++) 
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            result += texture(uTextureSSAO, vTexture + offset).r;
        }
    }
    shade = result / (3.0 * 3.0);
}