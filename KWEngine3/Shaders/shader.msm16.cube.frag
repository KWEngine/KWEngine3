#version 400 core
in vec4 gPosition;
in vec2 gTexture;

uniform vec3 uLightPosition;
uniform vec2 uNearFar;
uniform sampler2D uTextureAlbedo;

out vec4 map;

const mat4 quantizationMatrix2 = mat4(
									 -2.07224649 , 13.7948857237, 0.105877704, 9.7924062118,
									 32.23703778 , -59.4683975703, -1.9077466311, -33.7652110555,
									 -68.571074599 , 82.0359750338, 9.3496555107 , 47.9456096605,
									 39.3703274134, -35.364903257, -6.6543490743, -23.9728048165
									 );
const float offsetZero = 0.035955884801;

void main()
{
	if(texture(uTextureAlbedo,gTexture).w < 0.5)
		discard;

    float lightDistance = length(gPosition.xyz - uLightPosition);
	float z = (lightDistance - uNearFar.x) / (uNearFar.y - uNearFar.x);

    float zSq = z * z;
	vec4 b_unbiased = vec4(z, zSq, z * zSq, zSq * zSq);
	map = quantizationMatrix2 * b_unbiased;
	map.x += offsetZero;

    gl_FragDepth = lightDistance / uNearFar.y;
}  