#version 400 core

in vec2 vTexture;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

uniform vec4 uColorTint;
uniform sampler2D uTextureAlbedo;

vec4 getAlbedo()
{
    return texture(uTextureAlbedo, vTexture);
}

void main()
{
	vec4 albedo = getAlbedo();
    if(albedo.w <= 0.0)
    {
        discard;
    }

    float bloomR = 0.0;
    float bloomG = 0.0;
    float bloomB = 0.0;
    if(uColorTint.x * uColorTint.w > 1.0)
        bloomR = uColorTint.x * uColorTint.w - 1.0;
    if(uColorTint.y * uColorTint.w > 1.0)
        bloomG = uColorTint.y * uColorTint.w - 1.0;
    if(uColorTint.z * uColorTint.w > 1.0)
        bloomB = uColorTint.z * uColorTint.w - 1.0;
    color = vec4(uColorTint.x, uColorTint.y, uColorTint.z, uColorTint.w);
    bloom = vec4(bloomR, bloomG, bloomB, 1.0); //max(0, uColorTint.w - 1.0));
}

