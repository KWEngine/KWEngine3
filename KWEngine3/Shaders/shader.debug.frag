#version 400 core

in vec2 vTexture;

layout(location = 0) out vec4 color;

uniform sampler2D uTexture;
uniform ivec4 uOptions; // x = type, y = value, z = near, w = far

float linearizeDepth(float d)
{
	return pow(d, uOptions.w * 0.01);
}

void main()
{
	int val = uOptions.y;
	vec4 color2D = texture(uTexture, vTexture);
	color = vec4(0.0, 0.0, 0.0, 1.0);

	// Debug Modes:
    // 1 = Depth
    // 2 = Color
    // 3 = Normals
    // 4 = SSAO
    // 5 = Bloom
    // 6 = MetallicRoughness
	if(uOptions.x == 1)
	{
		float depth = linearizeDepth(color2D.x);
		color = vec4(depth, depth, depth, 1.0);
	}
	else if(uOptions.x == 2)
	{
		color = color2D;
	}
	else if(uOptions.x == 3)
	{
		vec3 tmp = (color2D * 0.5 + 0.5).xyz;
		color = vec4(tmp.x, tmp.y, tmp.z, 1.0);
	}
	else if(uOptions.x == 4)
	{
		float ssao = color2D.x;
		color = vec4(ssao, ssao, ssao, 1.0);
	}
	else if(uOptions.x == 5) // bloom
	{
		color = vec4(color2D.x, color2D.y, color2D.z, 1.0); 
	}
	else if(uOptions.x == 6) // metallicroughness (x/y)
	{
		color = vec4(color2D.x, color2D.y, 0.0, 1.0);
	}
	else if(uOptions.x == 7)
	{
		color = color2D;	
	}
	else if(uOptions.x == 8)
	{
		color = color2D;	
	}
	else if(uOptions.x == 9)
	{
		color = color2D;	
	}
}