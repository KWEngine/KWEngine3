#version 400 core

in vec2 vTexture;

layout(location = 0) out float shade;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;
uniform sampler2D uTextureDepth;
uniform sampler2D uTextureNoise;
uniform mat4 uViewProjectionMatrixInverted;
uniform mat4 uProjectionMatrix;
uniform vec3 uKernel[64];
uniform vec2 uNoiseScale;
uniform vec2 uRadiusBias;

//const float radius = 0.5;
//const float bias = 0.025;

vec3 decodeNormal2(vec2 encoded) 
{
    // Rückskalierung in den Bereich [-1, 1]
    vec2 projected;
    projected.x = (encoded.x) * 2.0 - 1.0;
    projected.y = (encoded.y) * 2.0 - 1.0;

    // Rekonstruktion des Z-Werts
    float pSquared = dot(projected, projected);
    float z = 1.0 - pSquared;
    z = z > 0.0 ? sqrt(z) : 0.0; // Numerische Stabilität

    // Rekonstruktion des Normalvektors
    vec3 normal = vec3(projected * (1.0 + z), z);
    return normalize(normal);
}

vec3 decodeNormal(vec2 encodedNormal) {
    vec2 f = encodedNormal * 2.0 - 1.0;
    float g = dot(f, f);
    float z = 1.0 - g * 0.5;
    vec3 normal;
    normal.xy = f * sqrt(1.0 - z * z);
    normal.z = z;
    return normal;
}

vec3 getFragmentPosition()
{
    float depth = texture(uTextureDepth, vTexture).r * 2.0 - 1.0;
    vec4 clipSpaceCoordinate = vec4((vTexture) * 2.0 - 1.0, depth, 1.0);
    vec4 worldSpaceCoordinate = uViewProjectionMatrixInverted * clipSpaceCoordinate;
    worldSpaceCoordinate.xyz /= worldSpaceCoordinate.w;
    return worldSpaceCoordinate.xyz;
}

vec3 getFragmentPositionOffset(vec2 offset)
{
    float depth = texture(uTextureDepth, offset).r * 2.0 - 1.0;
    vec4 clipSpaceCoordinate = vec4((offset) * 2.0 - 1.0, depth, 1.0);
    vec4 worldSpaceCoordinate = uViewProjectionMatrixInverted * clipSpaceCoordinate;
    worldSpaceCoordinate.xyz /= worldSpaceCoordinate.w;
    return worldSpaceCoordinate.xyz;
}

void main()
{
    vec3 fragmentPosWorldSpace = getFragmentPosition();
    vec3 normal    = normalize(decodeNormal2(texture(uTextureNormal, vTexture).xy));
    vec3 randomVec = normalize(texture(uTextureNoise, vTexture * uNoiseScale).xyz);  

    vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN       = mat3(tangent, bitangent, normal);  

    float occlusion = 0.0;
    for(int i = 0; i < 64; i++)
    {
        // get sample position
        vec3 samplePos = TBN * uKernel[i]; // from tangent to view-space
        samplePos = fragmentPosWorldSpace + samplePos * uRadiusBias.x;
    
        vec4 offset = vec4(samplePos, 1.0);
        offset      = uProjectionMatrix * offset;    // from view to clip-space
        offset.xyz /= offset.w;               // perspective divide
        offset.xyz  = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0  

        float sampleDepth = getFragmentPositionOffset(offset.xy).z;

        float rangeCheck = smoothstep(0.0, 1.0, uRadiusBias.x / abs(fragmentPosWorldSpace.z - sampleDepth));
        occlusion += (sampleDepth >= samplePos.z + uRadiusBias.y ? 1.0 : 0.0) * rangeCheck; // 0.025 = optional bias (uniform?)
    }  
    occlusion = 1.0 - (occlusion / 64.0);

	shade = occlusion;
}