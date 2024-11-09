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
uniform ivec4 uTerrainData;
uniform sampler2D uTextureHeightMap;

out vec2 vTexture;
out vec3 vNormal;
out mat3 vTBN;

const vec3 BASENORMAL = vec3(0.0, 1.0, 0.0);

void main()
{
    float offset = 1.0 / gl_TessLevelInner[0];

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

    vec2 tho1 = th00 * (1.0 - (u + offset)) * (1.0 - (v + offset)) +
                th01 * (u + offset) * (1.0 - (v + offset)) + 
                th03 * (v + offset) * (1.0 - (u + offset)) +
                th02 * (u + offset) * (v + offset);
    vec2 tho2 = th00 * (1.0 - (u - offset)) * (1.0 - (v + offset)) +
                th01 * (u - offset) * (1.0 - (v + offset)) + 
                th03 * (v + offset) * (1.0 - (u - offset)) +
                th02 * (u - offset) * (v + offset);
    vec2 tho3 = th00 * (1.0 - (u - offset)) * (1.0 - (v - offset)) +
                th01 * (u - offset) * (1.0 - (v - offset)) + 
                th03 * (v - offset) * (1.0 - (u - offset)) +
                th02 * (u - offset) * (v - offset);
    vec2 tho4 = th00 * (1.0 - (u + offset)) * (1.0 - (v - offset)) +
                th01 * (u + offset) * (1.0 - (v - offset)) + 
                th03 * (v - offset) * (1.0 - (u + offset)) +
                th02 * (u + offset) * (v - offset);

    //ivec2 tSize = textureSize(uTextureHeightMap, 0);
    float height = textureLod(uTextureHeightMap, th, 0).r * uTerrainData.w;
    float heighto1 = textureLod(uTextureHeightMap, tho1, 0).r * uTerrainData.w;
    float heighto2 = textureLod(uTextureHeightMap, tho2, 0).r * uTerrainData.w;
    float heighto3 = textureLod(uTextureHeightMap, tho3, 0).r * uTerrainData.w;
    float heighto4 = textureLod(uTextureHeightMap, tho4, 0).r * uTerrainData.w;

    vec3 p00 = gl_in[0].gl_Position.xyz;         // right back (of untessellated?)
    vec3 p01 = gl_in[1].gl_Position.xyz;         // left back
    vec3 p02 = gl_in[2].gl_Position.xyz;         // left front
    vec3 p03 = gl_in[3].gl_Position.xyz;         // right front
    vec3 p =    p00 * (1.0 - u) * (1.0 - v) +
                p01 * u * (1.0 - v) + 
                p03 * v * (1.0 - u) +
                p02 * u * v;
    vec3 po1 =  p00 * (1.0 - (u + offset)) * (1.0 - (v + offset)) +
                p01 * (u + offset) * (1.0 - (v + offset)) + 
                p03 * (v + offset) * (1.0 - (u + offset)) +
                p02 * (u + offset) * (v + offset);
    vec3 po2 =  p00 * (1.0 - (u - offset)) * (1.0 - (v + offset)) +
                p01 * (u - offset) * (1.0 - (v + offset)) + 
                p03 * (v + offset) * (1.0 - (u - offset)) +
                p02 * (u - offset) * (v + offset);
    vec3 po3 =  p00 * (1.0 - (u - offset)) * (1.0 - (v - offset)) +
                p01 * (u - offset) * (1.0 - (v - offset)) + 
                p03 * (v - offset) * (1.0 - (u - offset)) +
                p02 * (u - offset) * (v - offset);
    vec3 po4 =  p00 * (1.0 - (u + offset)) * (1.0 - (v - offset)) +
                p01 * (u + offset) * (1.0 - (v - offset)) + 
                p03 * (v - offset) * (1.0 - (u + offset)) +
                p02 * (u + offset) * (v - offset);



    vec4 positionWorldSpace = uModelMatrix * vec4(p, 1.0);
    positionWorldSpace.xyz += BASENORMAL * height;

    // compute patch surface normal
    vec3 p_to_o1 = normalize(cross(cross((po1 + BASENORMAL * heighto1) - (p + BASENORMAL * height), BASENORMAL), (po1 + BASENORMAL * heighto1) - (p + BASENORMAL * height)));
    vec3 p_to_o2 = normalize(cross(cross((po2 + BASENORMAL * heighto2) - (p + BASENORMAL * height), BASENORMAL), (po2 + BASENORMAL * heighto2) - (p + BASENORMAL * height)));
    vec3 p_to_o3 = normalize(cross(cross((po3 + BASENORMAL * heighto3) - (p + BASENORMAL * height), BASENORMAL), (po3 + BASENORMAL * heighto3) - (p + BASENORMAL * height)));
    vec3 p_to_o4 = normalize(cross(cross((po4 + BASENORMAL * heighto4) - (p + BASENORMAL * height), BASENORMAL), (po4 + BASENORMAL * heighto4) - (p + BASENORMAL * height)));
    vec3 normalLighting = (p_to_o1 + p_to_o2 + p_to_o3 + p_to_o4) / 4.0;

    // calculate tangent and bitangent:
    vec3 tangent = vec3(1.0, 0.0, 0.0);
    vec3 bitangent = vec3(0.0, 0.0, 1.0);
    if(dot(normalLighting, BASENORMAL) < 1.0)
    {
        tangent = normalize(cross(normalLighting, BASENORMAL));
        bitangent = normalize(cross(tangent, normalLighting));
    }
    
    vec2 t00 = vTextureTE[0];
    vec2 t01 = vTextureTE[1];
    vec2 t02 = vTextureTE[2];
    vec2 t03 = vTextureTE[3];
    vec2 t =    t00 * (1.0 - u) * (1.0 - v) +
                t01 * u * (1.0 - v) + 
                t03 * v * (1.0 - u) +
                t02 * u * v;

    // ----------------------------------------------------------------------

    gl_Position = uViewProjectionMatrix * positionWorldSpace;
    vTexture = t;
    vNormal = normalLighting;
	vTBN = mat3(tangent, -bitangent, vNormal);
}