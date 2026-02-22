#version 400 core
in		vec2 vTexture;

uniform vec4        uColorTint;
uniform vec2		uColorHue;
uniform sampler2D   uTexture;

layout(location = 0) out vec4 color;
layout(location = 1) out vec4 bloom;

// 2025-06-13: added hue
vec3 hueShift(vec3 color, float hue)
{
	const vec3 k = vec3(0.57735, 0.57735, 0.57735);
	float cosAngle = cos(hue);
	return vec3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
}

void main()
{
	vec4 tex = texture(uTexture, vTexture);
	vec3 texTmp = hueShift(tex.xyz * uColorHue.y, uColorHue.x);
	tex.xyz = texTmp;
	color = (tex * vec4(uColorTint.xyz, 1.0)) * uColorTint.w;
	
	bloom.x = max(0.0, color.x * tex.w - 1.0);
	bloom.y = max(0.0, color.y * tex.w - 1.0);
	bloom.z = max(0.0, color.z * tex.w - 1.0);
	bloom.w = tex.w * uColorTint.w;

}