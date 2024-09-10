#version 400 core

layout(location = 0) out vec4 color;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 metallicRoughnessMetallicType;
layout(location = 3) out ivec2 idShadowCaster;
uniform int uIsSector;

void main()
{
	normal = vec3(0, -1, 0);
	metallicRoughnessMetallicType = vec3(0, 1, 0);
	idShadowCaster = ivec2(0,0);
	if(uIsSector > 0)
	{
		color = vec4(1.0, 0.0, 0.0, 1.0);
	}
	else
	{
		color = vec4(0.75, 0.75, 0.75, 1.0);
	}
}