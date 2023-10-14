#version 400 core

in		float vZ;
in		float vW;
in		vec2 vTexture;

const float s3 = sqrt(3.0) / 2.0;
const float s12 = sqrt(12.0) / -9.0;
const vec4 addition = vec4(0.5, 0.0, 0.5, 0.0);
const mat4 quantizationMatrix = mat4(
										 1.5,  0.0,   s3,  0.0, 
										 0.0,  4.0,  0.0,  0.5,
										-2.0,  0.0,  s12,  0.0,
										 0.0, -4.0,  0.0,  0.5
										);

const mat4 quantizationMatrix2 = mat4(
									 -2.07224649 , 13.7948857237, 0.105877704, 9.7924062118,
									 32.23703778 , -59.4683975703, -1.9077466311, -33.7652110555,
									 -68.571074599 , 82.0359750338, 9.3496555107 , 47.9456096605,
									 39.3703274134, -35.364903257, -6.6543490743, -23.9728048165
									 );
const mat4 quantizationMatrix2t = mat4(
									 -2.07224649 , 32.23703778, -68.571074599, 39.3703274134,
									 13.7948857237 , -59.4683975703, 82.0359750338, -35.364903257,
									 0.105877704 , -1.9077466311, 9.3496555107 , -6.6543490743,
									 9.7924062118, -33.7652110555, 47.9456096605, -23.9728048165
									 );

const float offsetZero = 0.035955884801;

uniform vec3 uNearFarSun; // 0 = point, -1 = sun, +1 = directional
uniform sampler2D uTextureAlbedo;

out vec4 map;

void main()
{
	if(texture(uTextureAlbedo,vTexture).w <= 0)
		discard;

	float z = vZ;
	if(uNearFarSun.z >= 0.0)
	{
		z = (vZ - uNearFarSun.x) / (uNearFarSun.y - uNearFarSun.x);
	}
	else
	{
		z = z / vW;
		z = z * 0.5 + 0.5;
	}
	float zSq = z * z;
	vec4 b_unbiased = vec4(z, zSq, z * zSq, zSq * zSq);
	map = quantizationMatrix2 * b_unbiased;
	map.x += offsetZero;
}