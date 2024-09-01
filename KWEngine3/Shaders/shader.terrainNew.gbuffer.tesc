// tessellation control shader
#version 400 core

// specify number of control points per patch output
// this value controls the size of the input and output arrays
layout (vertices=4) out;

// varying input from vertex shader
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

const float outer = 3;
const float inner = 3;

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
        vec3 deltaCam = uCamPosition - vPosition[gl_InvocationID];
        float delta = length(deltaCam);
        delta = max(16.0 - delta, 1);
        float tessLevel = int(delta);

       vec4 vPositionClipSpace = uViewProjectionMatrix * uModelMatrix * vec4(vPositionTE[gl_InvocationID], 1.0);
       vPositionClipSpace.xyz /= vPositionClipSpace.w;

       tessLevel = clamp((1.0 - vPositionClipSpace.z) * 160, 1, 16);

        gl_TessLevelOuter[0] = tessLevel;
        gl_TessLevelOuter[1] = tessLevel;
        gl_TessLevelOuter[2] = tessLevel;
        gl_TessLevelOuter[3] = tessLevel;
                               
        gl_TessLevelInner[0] = tessLevel;
        gl_TessLevelInner[1] = tessLevel;
    }
}
	