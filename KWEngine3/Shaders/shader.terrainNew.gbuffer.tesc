// tessellation control shader
#version 400 core

// specify number of control points per patch output
// this value controls the size of the input and output arrays
layout (vertices=4) out;

// input from vertex shader
in vec3 vPosition[];
in vec2 vTexture[];
in vec3 vNormal[];
in vec3 vTangent[];
in vec3 vBiTangent[];

// varying output to evaluation shader
out vec3 vPositionTE[];
out vec2 vTextureTE[];
out vec3 vNormalTE[];
out vec3 vTangentTE[];
out vec3 vBiTangentTE[];

uniform vec3 uCamPosition;
uniform vec3 uCamDirection;
uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform vec4 uTerrainData;

void main()
{
    
    // ----------------------------------------------------------------------
    // pass attributes through
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    vPositionTE[gl_InvocationID] = vPosition[gl_InvocationID];
    vTextureTE[gl_InvocationID] = vTexture[gl_InvocationID];
    vNormalTE[gl_InvocationID] = vNormal[gl_InvocationID];
    vTangentTE[gl_InvocationID] = vTangent[gl_InvocationID];
    vBiTangentTE[gl_InvocationID] = vBiTangent[gl_InvocationID];

    // ----------------------------------------------------------------------
    // invocation zero controls tessellation levels for the entire patch
    if (gl_InvocationID == 0)
    {
       vec4 vPositionWorldSpace =  uModelMatrix * vec4((vPositionTE[0] + vPositionTE[3]) * 0.5, 1.0);
       float delta = 1.0 / (length(vPositionWorldSpace.xyz - uCamPosition) * uTerrainData.w);
       int tessLevel = int(clamp(delta * 128, 1.0, 32.0));

        gl_TessLevelOuter[0] = tessLevel - (tessLevel % 2);
        gl_TessLevelOuter[1] = tessLevel - (tessLevel % 2);
        gl_TessLevelOuter[2] = tessLevel - (tessLevel % 2);
        gl_TessLevelOuter[3] = tessLevel - (tessLevel % 2);
                               
        gl_TessLevelInner[0] = tessLevel - (tessLevel % 2);
        gl_TessLevelInner[1] = tessLevel - (tessLevel % 2);
    }
}
	