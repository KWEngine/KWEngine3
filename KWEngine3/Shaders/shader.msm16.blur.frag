#version 400 core

out vec4 color;

in vec2 vTexture;

uniform sampler2D uTextureInput;
uniform int uAxis;

vec4 blur5(vec2 resolution, vec2 direction) {
  vec4 color = vec4(0.0);
  vec2 off1 = vec2(1.3333333333333333) * direction;
  color += texture(uTextureInput, vTexture) * 0.29411764705882354;
  color += texture(uTextureInput, vTexture + (off1 / resolution)) * 0.35294117647058826;
  color += texture(uTextureInput, vTexture - (off1 / resolution)) * 0.35294117647058826;
  return color; 
}

void main()
{
    vec2 txDimsF = textureSize(uTextureInput, 0);
    color = blur5(txDimsF, uAxis == 0 ? vec2(1.0, 0.0) : vec2(0.0, 1.0));
}