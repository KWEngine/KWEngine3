#version 400 core

in vec2 vTexture;

layout(location = 0) out vec4 color;

uniform samplerCube uTexture;
uniform sampler2D uTextureDepth;
uniform vec3 uCameraPosition;
uniform mat4 uViewProjectionMatrixInverted;

vec3 getFragmentPosition()
{
    float depth = texture(uTextureDepth, vTexture).r * 2.0 - 1.0;
    vec4 clipSpaceCoordinate = vec4(vTexture * 2.0 - 1.0, depth, 1.0);
    vec4 worldSpaceCoordinate = uViewProjectionMatrixInverted * clipSpaceCoordinate;
    worldSpaceCoordinate.xyz /= worldSpaceCoordinate.w;
    return worldSpaceCoordinate.xyz;
}

void main()
{
    vec3 fragmentWorldPos = getFragmentPosition();
	vec4 color3D = texture(uTexture, normalize(fragmentWorldPos - uCameraPosition));
	color = color3D;
}