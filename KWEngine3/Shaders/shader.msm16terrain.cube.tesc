// tessellation control shader
#version 400 core

// specify number of control points per patch output
// this value controls the size of the input and output arrays
layout (vertices=4) out;

// input from vertex shader
in vec3 vPosition[];
in vec3 vNormal[];
in vec2 vTextureHeight[];

// output to evaluation shader
out vec3 vPositionTE[];
out vec3 vNormalTE[];
out vec2 vTextureHeightTE[];

uniform vec3 uCamPosition;
uniform vec3 uCamDirection;
uniform mat4 uModelMatrix;
uniform ivec4 uTerrainData;

int getTLevel(vec3 dir, float dp)
{
    dp = step(0, dp); // if dp < 0 => 0, else 1
    float l = dot(dir, dir) + ((1 - dp) * 32768.0);

    if(l < 512)
    {
        return 32;
    }
    else if(l < 2048.0)
    {
        return 16;
    }
    else if(l < 8096.0)
    {
        return 8;
    }
    else if(l < 32768.0)
    {
        return 4;
    }
    else
    {
        return 1;
    }
}

void main()
{
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    vPositionTE[gl_InvocationID] = vPosition[gl_InvocationID];
    vTextureHeightTE[gl_InvocationID] = vTextureHeight[gl_InvocationID];
    vNormalTE[gl_InvocationID] = vNormal[gl_InvocationID];

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

        // 1 = no tessellation outer
        gl_TessLevelOuter[0] = getTLevel(camToRight, dotCamToRight); // right
        gl_TessLevelOuter[1] = getTLevel(camToBack, dotCamToBack); // back
        gl_TessLevelOuter[2] = getTLevel(camToLeft, dotCamToLeft); // left
        gl_TessLevelOuter[3] = getTLevel(camToFront, dotCamToFront); // front

        // 1 = no tesselation inner
        gl_TessLevelInner[0] = (gl_TessLevelOuter[0] + gl_TessLevelOuter[2]) * 0.5; // horizontally
        gl_TessLevelInner[1] = (gl_TessLevelOuter[1] + gl_TessLevelOuter[3]) * 0.5; // vertically
    }
}