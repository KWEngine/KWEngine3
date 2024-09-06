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
uniform vec4 uTerrainData;

int getTLevel(vec3 vPosWS)
{
    float l = length(vPosWS.xyz - uCamPosition);
    float delta = (8096.0 * 2.0) / (l * l);
    int tlevel = int(clamp(delta, 2.0, 16.0)); 
    tlevel = tlevel - tlevel % 2;
    return tlevel;
}

void main()
{
    
    // ----------------------------------------------------------------------
    // pass attributes through
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    vPositionTE[gl_InvocationID] = vPosition[gl_InvocationID];
    vTextureHeightTE[gl_InvocationID] = vTextureHeight[gl_InvocationID];
    vNormalTE[gl_InvocationID] = vNormal[gl_InvocationID];

    if (gl_InvocationID == 0)
    {
        vec4 vPositionWorldSpace = vec4(1);

        // left edge:
        vPositionWorldSpace =  uModelMatrix * vec4(vPositionTE[1], 1.0);
        int tessLevelLeft = getTLevel(vPositionWorldSpace.xyz);
        
       
        // right edge:
        vPositionWorldSpace =  uModelMatrix * vec4(vPositionTE[1], 1.0);
        int tessLevelRight = getTLevel(vPositionWorldSpace.xyz);
        
        // back edge
        vPositionWorldSpace =  uModelMatrix * vec4(vPositionTE[2], 1.0);
        int tessLevelBack = getTLevel(vPositionWorldSpace.xyz);

         // front edge
        vPositionWorldSpace =  uModelMatrix * vec4(vPositionTE[2], 1.0);
        int tessLevelFront = getTLevel(vPositionWorldSpace.xyz);

        int a = int((tessLevelLeft + tessLevelRight) * 0.5);
        int b = int((tessLevelFront + tessLevelBack) * 0.5);
        a = a + a % 2;
        b = b + b % 2;

        gl_TessLevelOuter[0] = a;
        gl_TessLevelOuter[1] = b;
        gl_TessLevelOuter[2] = a;
        gl_TessLevelOuter[3] = b;

        gl_TessLevelInner[0] = a; 
        gl_TessLevelInner[1] = a;
    }
    
}
	