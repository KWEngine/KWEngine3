#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;

uniform vec3 uCenter;
uniform float uRadiansY;
uniform mat4 uViewProjectionMatrix;
uniform mat4 uRotationMatrix;
uniform float uScale;

out vec2 vTexture;

void main()
{
	vTexture = aTexture;
	vec4 posTmp = uRotationMatrix * vec4(aPosition, 1.0);

	gl_Position = uViewProjectionMatrix * vec4(posTmp.x * uScale + uCenter.x, posTmp.y * uScale + uCenter.y, posTmp.z * uScale + uCenter.z, posTmp.w);
}