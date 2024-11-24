// tessellation control shader
#version 400 core

// specify number of control points per patch output
// this value controls the size of the input and output arrays
layout (vertices=4) out;

// input from vertex shader
in vec3 vPosition[];
in vec2 vTexture[];
in vec2 vTextureHeight[];
in vec3 vNormal[];
in vec3 vTangent[];
in vec3 vBiTangent[];

// output to evaluation shader
out vec3 vPositionTE[];
out vec2 vTextureTE[];
out vec2 vTextureHeightTE[];
out vec3 vNormalTE[];
out vec3 vTangentTE[];
out vec3 vBiTangentTE[];

uniform vec3 uCamPosition;
uniform vec3 uCamDirection;
uniform mat4 uModelMatrix;
uniform mat4 uViewProjectionMatrix;
uniform ivec4 uTerrainData;
uniform int uTerrainThreshold;

int getTLevel(vec3 dir, float dp)
{
    //dp = step(0, dp); // if dp < 0 => 0, else 1
    dp = 1.0;
    float l = dot(dir, dir) + ((1 - dp) * (uTerrainThreshold * 32));

    if(l < uTerrainThreshold) // 1024
    {
        return 32;
    }
    else if(l < uTerrainThreshold * 4) // 4096
    {
        return 16;
    }
    else if(l < uTerrainThreshold * 16) // 16192
    {
        return 8;
    }
    else if(l < uTerrainThreshold * 32) // 32768
    {
        return 4;
    }
    else
    {
        return 2;
    }
}

void main()
{
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    vPositionTE[gl_InvocationID] = vPosition[gl_InvocationID];
    vTextureTE[gl_InvocationID] = vTexture[gl_InvocationID];
    vTextureHeightTE[gl_InvocationID] = vTextureHeight[gl_InvocationID];
    vNormalTE[gl_InvocationID] = vNormal[gl_InvocationID];
    vTangentTE[gl_InvocationID] = vTangent[gl_InvocationID];
    vBiTangentTE[gl_InvocationID] = vBiTangent[gl_InvocationID];

    if (gl_InvocationID == 0)
    {
        //vPosition[0] => right back
        //vPosition[1] => left back
        //vPosition[2] => left front
        //vPosition[3] => right front
        vec4 pBack = uModelMatrix * vec4((vPosition[0] + vPosition[1]) * 0.5, 1.0);
        vec4 pFront = uModelMatrix * vec4((vPosition[2] + vPosition[3]) * 0.5, 1.0);
        vec4 pLeft = uModelMatrix * vec4((vPosition[1] + vPosition[2]) * 0.5, 1.0);
        vec4 pRight = uModelMatrix * vec4((vPosition[0] + vPosition[3]) * 0.5, 1.0);

        vec3 camToLeft = pLeft.xyz - uCamPosition;
        vec3 camToRight = pRight.xyz - uCamPosition;
        vec3 camToBack = pBack.xyz - uCamPosition;
        vec3 camToFront = pFront.xyz - uCamPosition;

        float dotCamToLeft  = dot(camToLeft, uCamDirection);
        float dotCamToRight = dot(camToRight, uCamDirection);
        float dotCamToBack  = dot(camToBack, uCamDirection);
        float dotCamToFront = dot(camToFront, uCamDirection);

        float dp = max(max(max(dotCamToLeft, dotCamToRight), dotCamToBack), dotCamToFront);

        // 1 = no tessellation outer
        gl_TessLevelOuter[0] = getTLevel(camToRight, dp); // right
        gl_TessLevelOuter[1] = getTLevel(camToBack, dp); // back
        gl_TessLevelOuter[2] = getTLevel(camToLeft, dp); // left
        gl_TessLevelOuter[3] = getTLevel(camToFront, dp); // front

        // 1 = no tesselation inner
        gl_TessLevelInner[0] = (gl_TessLevelOuter[0] + gl_TessLevelOuter[2]) * 0.5; // horizontally
        gl_TessLevelInner[1] = (gl_TessLevelOuter[1] + gl_TessLevelOuter[3]) * 0.5; // vertically
    }
    
}
	