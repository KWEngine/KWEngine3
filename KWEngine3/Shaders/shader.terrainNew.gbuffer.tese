#version 400 core
layout (quads, equal_spacing, ccw) in;

in vec3 vPositionTE[];
in vec2 vTextureTE[];
in vec2 vTextureHeightTE[];
in vec3 vNormalTE[];
in vec3 vTangentTE[];
in vec3 vBiTangentTE[];

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec4 uTerrainData;
uniform sampler2D uTextureHeightMap;

out vec2 vTexture;
out vec3 vNormal;
out mat3 vTBN;

void main()
{
	// get patch coordinate
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;
  
    vec2 t00 = vTextureTE[0];
    vec2 t01 = vTextureTE[1];
    vec2 t02 = vTextureTE[2];
    vec2 t03 = vTextureTE[3];

    vec2 t =    t00 * (1.0 - u) * (1.0 - v) +
                t01 * u * (1.0 - v) + 
                t03 * v * (1.0 - u) +
                t02 * u * v;

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
  
    vec3 normal =   vNormalTE[0] * (1.0 - u) * (1.0 - v) +
                    vNormalTE[1] * u * (1.0 - v) + 
                    vNormalTE[3] * v * (1.0 - u) +
                    vNormalTE[2] * u * v;
    

    vec3 p =    p00 * (1.0 - u) * (1.0 - v) +
                p01 * u * (1.0 - v) + 
                p03 * v * (1.0 - u) +
                p02 * u * v;

    vec4 positionWorldSpace = uModelMatrix * vec4(p, 1.0);

    positionWorldSpace.xyz += normal * height;

    // compute patch surface normal
    vec3 uVec = p00 - p01;
    vec3 vVec = p02 - p00;
    vec3 normalLighting = normalize(cross(vVec, uVec));

    vec3 tangent = vTangentTE[0] - normalLighting * dot( vTangentTE[0], normalLighting ); // orthonormalization ot the tangent vectors
    vec3 bitangent = vBiTangentTE[0] - normalLighting * dot( vBiTangentTE[0], normalLighting ); // orthonormalization of the binormal vectors to the normal vector 
    bitangent = bitangent - tangent * dot( bitangent, tangent ); // orthonormalization of the binormal vectors to the tangent vector
    


    // ----------------------------------------------------------------------
    // output patch point position in clip space
    gl_Position = uViewProjectionMatrix * positionWorldSpace;
    vTexture = t;
    vNormal = normalize((uNormalMatrix * vec4(normalLighting, 0.0)).xyz);
    tangent = normalize((uNormalMatrix * vec4(tangent, 0.0)).xyz);
    bitangent = normalize((uNormalMatrix * vec4(bitangent, 0.0)).xyz);
	vTBN = mat3(tangent, bitangent, vNormal);
}