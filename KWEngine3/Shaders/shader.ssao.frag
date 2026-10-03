#version 400 core

in vec2 vTexture;

layout(location = 0) out float shade;

uniform sampler2D uTextureNormal;
uniform sampler2D uTextureDepth;
uniform sampler2D uTextureNoise;
uniform mat3 uViewMatrix3;      // world -> view rotation (G-buffer normals are world space)
uniform vec4 uProjectionParams; // x = 0.5 * P00, y = 0.5 * P11, z = -0.5 * P32, w = 0.5 * (P22 - 1)
uniform vec3 uKernel[64];
uniform uint uKernelSize;
uniform vec2 uRadiusBias;

// Assumes a symmetric perspective projection (Matrix4.CreatePerspectiveFieldOfView).
float getViewZ(float depth)
{
    return uProjectionParams.z / (depth + uProjectionParams.w);
}

vec3 decodeNormalFromRG16F(vec2 enc)
{
    vec3 n = vec3(enc, 1.0 - abs(enc.x) - abs(enc.y));
    if (n.z < 0.0)
    {
        n.xy = (1.0 - abs(n.yx)) * vec2(n.x >= 0.0 ? 1.0 : -1.0, n.y >= 0.0 ? 1.0 : -1.0);
    }
    return normalize(n);
}

void main()
{
    // The SSAO target may be smaller than the G-buffer: fetch one exact G-buffer texel
    // and reconstruct the position for that texel's center.
    vec2 gBufferSize = vec2(textureSize(uTextureDepth, 0));
    ivec2 texel = ivec2(vTexture * gBufferSize);
    float depth = texelFetch(uTextureDepth, texel, 0).r;
    if (depth >= 1.0)
    {
        shade = 1.0; // sky
        return;
    }

    vec2 uv = (vec2(texel) + 0.5) / gBufferSize;
    float z = getViewZ(depth);
    vec3 position = vec3((uv - 0.5) / uProjectionParams.xy * -z, z);
    vec3 normal = normalize(uViewMatrix3 * decodeNormalFromRG16F(texelFetch(uTextureNormal, texel, 0).xy));
    vec3 randomVec = texelFetch(uTextureNoise, ivec2(gl_FragCoord.xy) & 3, 0).xyz;

    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    mat3 TBN = mat3(tangent, cross(normal, tangent), normal) * uRadiusBias.x;

    float occlusion = 0.0;
    int kernelSize = int(uKernelSize);
    for (int i = 0; i < kernelSize; i++)
    {
        vec3 samplePos = position + TBN * uKernel[i];
        vec2 sampleUV = uProjectionParams.xy * samplePos.xy / -samplePos.z + 0.5;
        float sampleZ = getViewZ(texture(uTextureDepth, sampleUV).r);

        float rangeCheck = smoothstep(0.0, 1.0, uRadiusBias.x / abs(position.z - sampleZ));
        occlusion += (sampleZ >= samplePos.z + uRadiusBias.y ? rangeCheck : 0.0);
    }

    shade = 1.0 - occlusion / float(kernelSize);
}
