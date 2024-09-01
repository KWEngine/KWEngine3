#version 400 core
layout (quads, equal_spacing, ccw) in;

in vec3 vPositionTE[];
in vec2 vTextureTE[];
in vec3 vNormalTE[];
in vec3 vTangentTE[];
in vec3 vBiTangentTE[];

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform sampler2D uTextureHeightMap;


void main()
{
	// get patch coordinate
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;
    /*
	// retrieve control point texture coordinates
    vec2 t00 = vTextureTE[0];
    vec2 t01 = vTextureTE[1];
    vec2 t10 = vTextureTE[2];
    vec2 t11 = vTextureTE[3];

    // bilinearly interpolate texture coordinate across patch
    vec2 t0 = (t01 - t00) * u + t00;
    vec2 t1 = (t11 - t10) * u + t10;
    vec2 texCoord = (t1 - t0) * v + t0;
    */

    vec2 t00 = vTextureTE[0];
    vec2 t01 = vTextureTE[1];
    vec2 t02 = vTextureTE[2];
    vec2 t03 = vTextureTE[3];

    vec2 t =    t00 * (1.0 - u) * (1.0 - v) +
                t01 * u * (1.0 - v) + 
                t03 * v * (1.0 - u) +
                t02 * u * v;

    // lookup texel at patch coordinate for height and scale + shift as desired
    float height = texture(uTextureHeightMap, t).r * 0.0;

    // ----------------------------------------------------------------------
    // retrieve control point position coordinates
    /*
    vec4 p00 = gl_in[0].gl_Position;
    vec4 p01 = gl_in[1].gl_Position;
    vec4 p10 = gl_in[2].gl_Position;
    vec4 p11 = gl_in[3].gl_Position;
    */
    vec4 p00 = gl_in[0].gl_Position;
    vec4 p01 = gl_in[1].gl_Position;
    vec4 p02 = gl_in[2].gl_Position;
    vec4 p03 = gl_in[3].gl_Position;
    /*
    // compute patch surface normal
    vec4 uVec = p01 - p00;
    vec4 vVec = p10 - p00;
    vec4 normal = normalize( vec4(cross(vVec.xyz, uVec.xyz), 0) );

    // bilinearly interpolate position coordinate across patch
    vec4 p0 = (p01 - p00) * u + p00;
    vec4 p1 = (p11 - p10) * u + p10;
    vec4 p = (p1 - p0) * v + p0;

    // displace point along normal
    p += normal * height;
    */

    vec4 p =    p00 * (1.0 - u) * (1.0 - v) +
                p01 * u * (1.0 - v) + 
                p03 * v * (1.0 - u) +
                p02 * u * v;

    // ----------------------------------------------------------------------
    // output patch point position in clip space
    gl_Position = uViewProjectionMatrix * uModelMatrix * p;

	/*
	vec3 vTangent = normalize((uNormalMatrix * totalTangent).xyz);
	vec3 vBiTangent = normalize((uNormalMatrix * totalBiTangent).xyz);
	vTBN = mat3(vTangent.xyz, vBiTangent.xyz, vNormal.xyz);
	*/
}