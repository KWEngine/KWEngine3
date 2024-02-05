#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;
layout(location = 5) in vec3 aColor;

uniform mat4 uViewProjectionMatrix;
uniform vec4 uPlayerPosShadowCaster;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec3 uPatchSizeTime;
uniform ivec2 uNXNZ;
uniform vec2 uDXDZ;
uniform int uInstanceCount;
uniform vec2 uNoise[256];

out vec4 vPosition;
out vec2 vTexture;
out vec3 vNormal;
out mat3 vTBN;
out vec3 vColor;

#define M_PIHALF 3.141592 / 2.0
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
	vec3 axis = normalize(vec3(((gl_InstanceID % 256) - 128) / 768.0, 1.0, (((gl_InstanceID + 128) % 256)) - 128)/ 768.0);
	float swayFactor = sin(gl_InstanceID + uPatchSizeTime.z);
	mat3 rotMat = rotationMatrix(axis, swayFactor * (aPosition.y / 10.0));

	vec3 offsetXZ = vec3(uDXDZ.x * (gl_InstanceID % uNXNZ.x) + uNoise[gl_InstanceID % 256].x, 0.0, uDXDZ.y * int(gl_InstanceID / uNXNZ.x) + uNoise[gl_InstanceID % 256].y);
	vec3 center = vec3(-uDXDZ.x * (uNXNZ.x - 1) / 2.0, 0, -uDXDZ.y * (uNXNZ.y - 1) / 2.0);

	vec3 positionRandomized = rotMat * aPosition;
	//positionRandomized += vec3(randVertexOffset.x, 0.0, randVertexOffset.y) * (aPosition.y / 100) * swayFactor;

	vec4 totalLocalPos = vec4(positionRandomized + center + offsetXZ, 1.0);
	vec4 totalNormal = vec4(aNormal, 0.0);
	vec4 totalTangent = vec4(aTangent, 0.0);
	vec4 totalBiTangent = vec4(aBiTangent, 0.0);
	
	gl_Position = uViewProjectionMatrix * uModelMatrix * totalLocalPos;
	vPosition = uModelMatrix * totalLocalPos;
	vNormal = normalize((uNormalMatrix * totalNormal).xyz);
	vec3 tangent = normalize((uNormalMatrix * totalTangent).xyz);
	vec3 biTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTBN = mat3(tangent.xyz, biTangent.xyz, vNormal.xyz);

	vColor = aColor;
}