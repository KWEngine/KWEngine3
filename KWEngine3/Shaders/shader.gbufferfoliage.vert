#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform mat4 uViewProjectionMatrix;
uniform vec4 uPlayerPosShadowCaster;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec3 uPatchSizeTime;
uniform ivec2 uNXNZ;
uniform vec3 uDXDZSway;
uniform int uInstanceCount;
uniform vec2 uNoise[256];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out vec3 vColor;

#define M_PIHALF 3.141592 / 2.0
#define M_PIOCT 3.141592 / 8.0
#define M_PI32 3.141592 / 32.0
#define M_PI 3.141592

mat3 rotationMatrix(vec3 axis, float angle) 
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat3(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c        
                );
}

void main()
{
	vec3 axis = normalize(vec3(((gl_InstanceID % 256) - 128) / 768.0, 2.0, (((gl_InstanceID + 128) % 256)) - 128)/ 768.0);
	float swayFactor = (sin(gl_InstanceID + uPatchSizeTime.z + uNoise[gl_InstanceID % 512].x) + 1) * 0.5;
	mat3 rotMat = rotationMatrix(axis, swayFactor * (aPosition.y * M_PIHALF * uDXDZSway.z));

	vec3 offsetXZ = vec3(uDXDZSway.x * (gl_InstanceID % uNXNZ.x) + uNoise[gl_InstanceID % 256].x, 0.0, uDXDZSway.y * int(gl_InstanceID / uNXNZ.x) + uNoise[gl_InstanceID % 256].y);
	vec3 center = vec3(-uDXDZSway.x * (uNXNZ.x - 1) / 2.0, 0, -uDXDZSway.y * (uNXNZ.y - 1) / 2.0);

	vec3 positionRandomized = (uModelMatrix * mat4(rotMat) * vec4(aPosition, 1.0)).xyz;

	vec4 totalLocalPos = vec4(positionRandomized + center + offsetXZ, 1.0);
	vec4 totalNormal = vec4(normalize(rotMat * aNormal), 0.0);
	vec4 totalTangent = vec4(normalize(rotMat * aTangent), 0.0);
	vec4 totalBiTangent = vec4(normalize(rotMat * aBiTangent), 0.0);
	
	gl_Position = uViewProjectionMatrix * totalLocalPos;
	vPosition = uModelMatrix * totalLocalPos;
	vTexture = vec2(aTexture.x, 1.0 - aTexture.y);
	vNormal = normalize((uNormalMatrix * totalNormal).xyz);
	vColor = vec3(1.0); //min((pow(1.0 - aPosition.y, 3.0) * (-1.0) + 1.9) * vec3(1.0), vec3(1.0));
}