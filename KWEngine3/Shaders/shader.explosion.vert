#version 400 core

#define M_PIHALF 3.141592 / 2.0
#define M_PI 3.141592
#define M_PIE 3.141592 * 2.0

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

out vec4 vPosition;
out vec3 vNormal;

uniform mat4 uViewProjectionMatrix;
uniform float uTime;
uniform vec3 uSpreadSizeLength;
uniform float uNumber;
uniform vec3 uPosition;
uniform int uAlgorithm;
uniform int uTowardsIndex;
uniform vec2 uAxes[512];
uniform vec3 uDirection;
uniform mat4 uDirectionMatrix;

const vec3 AXES[22] = vec3[] (
            vec3(1,0,0),
            vec3(0,1,0),
            vec3(0,0,1),
            vec3(-1,0,0),
            vec3(0,-1,0),
            vec3(0,0,-1),

            vec3(0.707107,0.707107,0),   // right       up
            vec3(0.577351,0.577351,0.577351),   // right front up
            vec3(0,0.707107,0.707107),   // front       up
            vec3(-0.577351,0.577351,0.577351),  // left front  up
            vec3(-0.707107,0.707107,0),  // left        up
            vec3(-0.577351,0.577351,-0.577351), // left back   up
            vec3(0,0.707107,-0.707107),  // back        up
            vec3(0.577351,0.577351,-0.577351),  // right back  up

            vec3(0.707107,-0.707107,0),   // right       down
            vec3(0.707107,-0.707107,0.707107),   // right front down
            vec3(0,-0.707107,0.707107),   // front       down
            vec3(-0.577351,-0.577351,0.577351),  // left front  down
            vec3(-0.707107,-0.707107,0),  // left        down
            vec3(-0.577351,-0.577351,-0.577351), // left back   down
            vec3(0,-0.707107,-0.707107),  // back        down
            vec3(0.577351,-0.577351,-0.577351)  // right back  down
        );

mat4 rotationMatrix(vec3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

mat4 rotationMatrixToY = mat4(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1);

mat4 rotationMatrixY(float angle)
{
    vec3 axis = vec3(0.0, 1.0, 0.0);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(c, 0, s, 0,
                0, oc * 1.0 * 1.0 + c, 0, 0,
                -s, 0, c, 0,
                0, 0, 0, 1);
}

vec3 rotateVectorY(vec3 v, float angle)
{
    return vec3(cos(angle) * v.x - sin(angle) * v.z, v.y, sin(angle) * v.x + cos(angle) *v.z);
}

void main()
{
    float instancePercent = gl_InstanceID / (uNumber + 1);
    vec4 axis = vec4(AXES[int(uAxes[gl_InstanceID].x)], uAxes[gl_InstanceID].y);
    mat4 rotation = rotationMatrix(axis.xyz, instancePercent * (1.95 * M_PI));
    vec3 lookAt = normalize(vec3(rotation[0][0], rotation[1][0], rotation[2][0]));
	mat4 modelMatrix = mat4(1.0);

    float sizeFactor = max(pow(sin(2.0 * uTime + 1.25), 4.0), 0.0);
    modelMatrix[0][0] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[0][1] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[0][2] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[1][0] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[1][1] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[1][2] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[2][0] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[2][1] *= sizeFactor * uSpreadSizeLength.y * axis.w;
    modelMatrix[2][2] *= sizeFactor * uSpreadSizeLength.y * axis.w;

    if(uTowardsIndex == 0)
    {
        modelMatrix = rotation * modelMatrix;
    }
    else if(uTowardsIndex == 1)
    {
        modelMatrix = rotationMatrixToY * modelMatrix;
    }
    

    if(uAlgorithm == 0) // spread
    {
        modelMatrix[3][0] = uPosition.x + uSpreadSizeLength.x * uTime * lookAt.x * axis.w;
        modelMatrix[3][1] = uPosition.y + uSpreadSizeLength.x * uTime * lookAt.y * axis.w;
        modelMatrix[3][2] = uPosition.z + uSpreadSizeLength.x * uTime * lookAt.z * axis.w;
    }
    else if(uAlgorithm == 1) // wind up
    {
      
        modelMatrix[3][0] = uPosition.x + uSpreadSizeLength.x * uTime * lookAt.x * axis.w + lookAt.x * sin(uTime * 1.5 * M_PI);
        modelMatrix[3][1] = uPosition.y + uSpreadSizeLength.x * uTime * abs(lookAt.y) * axis.w + uSpreadSizeLength.z * uTime;
        modelMatrix[3][2] = uPosition.z + uSpreadSizeLength.x * uTime * lookAt.z * axis.w + lookAt.z * sin(uTime * 1.5 * M_PI);

        modelMatrix = uDirectionMatrix * modelMatrix;
    }
    else if(uAlgorithm == 2) // whirlwind up
    {
        modelMatrix[3][0] = 0.5 * uSpreadSizeLength.x * sin(uTime * M_PI) * (1.0 - lookAt.x) * axis.w;
        modelMatrix[3][1] = uPosition.y + uSpreadSizeLength.x * uTime * abs(lookAt.y) * axis.w + uSpreadSizeLength.z * uTime;
        modelMatrix[3][2] = 0.5 * uSpreadSizeLength.x * sin(uTime * M_PI) *(1.0 - lookAt.z) * axis.w;

        vec3 tmp = vec3(modelMatrix[3][0], modelMatrix[3][1], modelMatrix[3][2]);
        tmp = rotateVectorY(tmp, instancePercent * uTime * M_PIE * 2.0);

        modelMatrix[3][0] = uPosition.x + tmp.x;
        modelMatrix[3][1] = uPosition.y + tmp.y;
        modelMatrix[3][2] = uPosition.z + tmp.z;

        modelMatrix = uDirectionMatrix * modelMatrix;
    }
    else if(uAlgorithm == 3) // exhaust
    {
        modelMatrix[3][0] = uSpreadSizeLength.x * lookAt.x * axis.w - lookAt.x * 0.5 * uTime;
        modelMatrix[3][1] = uTime * abs(lookAt.y) * axis.w + uSpreadSizeLength.z * uTime;
        modelMatrix[3][2] = uSpreadSizeLength.x * lookAt.z * axis.w - lookAt.z * 0.5 * uTime;

        modelMatrix = uDirectionMatrix * modelMatrix;
    }
    else // spark
    {
        float downfactor = smoothstep(0.1, 1.0, uTime); 

        modelMatrix[3][0] = uPosition.x + uSpreadSizeLength.x * uTime * lookAt.x * axis.w + uDirection.x * uSpreadSizeLength.z * uTime;
        modelMatrix[3][1] = uPosition.y + uSpreadSizeLength.x * uTime * lookAt.y * axis.w + uDirection.y * uSpreadSizeLength.z * uTime - 1.0 * uSpreadSizeLength.z * downfactor;
        modelMatrix[3][2] = uPosition.z + uSpreadSizeLength.x * uTime * lookAt.z * axis.w + uDirection.z * uSpreadSizeLength.z * uTime;
    }

    
    
    mat4 mvp = uViewProjectionMatrix * modelMatrix;
    vPosition = modelMatrix * vec4(aPosition, 1.0);
    vNormal = (modelMatrix * vec4(aNormal, 0.0)).xyz;
	gl_Position = mvp * vec4(aPosition, 1.0);
}