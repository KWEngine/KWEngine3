#version 400 core
layout (quads, equal_spacing, ccw) in;

in vec3 vPositionTE[];
in vec3 vNormalTE[];
in vec2 vTextureHeightTE[];

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform ivec4 uTerrainData;
uniform sampler2D uTextureHeightMap;

out float vZ;
out float vW;

void main()
{
	// get patch coordinate
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;

    vec2 th00 = vTextureHeightTE[0];
    vec2 th01 = vTextureHeightTE[1];
    vec2 th02 = vTextureHeightTE[2];
    vec2 th03 = vTextureHeightTE[3];

    vec2 th =   th00 * (1.0 - u) * (1.0 - v) +
                th01 * u * (1.0 - v) + 
                th03 * v * (1.0 - u) +
                th02 * u * v;


    float height = texture(uTextureHeightMap, th).r * uTerrainData.w;

    vec3 p00 = gl_in[0].gl_Position.xyz;
    vec3 p01 = gl_in[1].gl_Position.xyz;
    vec3 p02 = gl_in[2].gl_Position.xyz;
    vec3 p03 = gl_in[3].gl_Position.xyz;

    vec3 p =    p00 * (1.0 - u) * (1.0 - v) +
                p01 * u * (1.0 - v) + 
                p03 * v * (1.0 - u) +
                p02 * u * v;

    vec3 normal =   vNormalTE[0] * (1.0 - u) * (1.0 - v) +
                    vNormalTE[1] * u * (1.0 - v) + 
                    vNormalTE[3] * v * (1.0 - u) +
                    vNormalTE[2] * u * v;

    vec4 positionWorldSpace = uModelMatrix * vec4(p, 1.0);

    positionWorldSpace.xyz += normal * height;

    vec4 transformedPosition = uViewProjectionMatrix * positionWorldSpace;
	vZ = transformedPosition.z;
	vW = transformedPosition.w;

    // ----------------------------------------------------------------------
    // output patch point position in clip space
    gl_Position = uViewProjectionMatrix * positionWorldSpace;
}