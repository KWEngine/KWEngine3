#version 400 core

layout(location = 0) out vec4 fragmentColor;

in vec2 vTexture;

uniform vec3 uColor;
uniform sampler2D uTexture;

void main()
{
	fragmentColor = vec4(uColor, 1.0) * texture(uTexture, vTexture);
}