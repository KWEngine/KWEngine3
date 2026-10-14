#version 400 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform mat4 uViewProjectionMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform vec3 uPatchSizeTime;
uniform ivec2 uNXNZ;
uniform vec4 uDXDZSwayRound;
uniform vec2 uNoise[256];
uniform vec3 uTerrainScale;
uniform vec3 uTerrainPosition;
uniform sampler2D uTerrainHeightMap;
uniform sampler2D uNoiseMap;
uniform vec3 uCameraPosition;
uniform ivec4 uCell;       // x = first column, y = first row, z = columns, w = rows (edge cells are cut off by the patch border)
uniform ivec2 uCellLength; // x = edge length of the cell in plants (power of two), y = log2 of the edge length
uniform vec3 uLOD;        // x = distance up to which a plant of nominal size is at least the minimum pixel width wide,
                          // y = draw distance, z = width of the fade band before the draw distance

out vec2 vTexture;
out vec3 vColor;
out mat3 vTBN;
out float vNormalBend;

#define M_PIHALF (3.141592 / 2.0)
#define M_PI 3.141592

const float NORMALBEND_NEAR = 0.75;
const float NORMALBEND_FAR = 0.9;
const float LOD_KEEP_MIN = 1.0 / 16.0;  // must match LOD_KEEP_MIN in RendererGBufferFoliage
const float LOD_KEEP_FADE = 0.3;        // must match LOD_KEEP_FADE in RendererGBufferFoliage

float rand(vec2 co)
{
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

float hashToUnitFloat(int value)
{
    uint h = uint(value) * 747796405u + 2891336453u;
    h = ((h >> ((h >> 28u) + 4u)) ^ h) * 277803737u;
    h = (h >> 22u) ^ h;
    return float(h) * (1.0 / 4294967296.0);
}

mat3 rotationMatrix(vec3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;

    return mat3(
        oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
        oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
        oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c
    );
}

mat4 scaleMatrix(float scaleFactor)
{
    return mat4(
        scaleFactor, 0, 0, 0,
        0, scaleFactor, 0, 0,
        0, 0, scaleFactor, 0,
        0, 0, 0, 1
    );
}

float getHeightFromTexture(vec3 vertexPos)
{
    float height;
    if(uTerrainScale.x == 0.0)
    {
        height = 0.0;
    }
    else
    {
        float uvX = clamp((vertexPos.x - uTerrainPosition.x + (uTerrainScale.x + 1.0) * 0.5) / (uTerrainScale.x + 1.0), 0.0, 1.0);
        float uvZ = clamp((vertexPos.z - uTerrainPosition.z + (uTerrainScale.z + 1.0) * 0.5) / (uTerrainScale.z + 1.0), 0.0, 1.0);

        vec4 texColor = textureLod(uTerrainHeightMap, vec2(uvX, uvZ), 0.0);
        height = ((texColor.r + texColor.g + texColor.b) / 3.0) * uTerrainScale.y + uTerrainPosition.y;
    }
    return height;
}

void main()
{
    int bitCount = uCellLength.y * 2;
    uint reversed = bitCount > 0 ? bitfieldReverse(uint(gl_InstanceID)) >> uint(32 - bitCount) : 0u;
    int lx = 0;
    int lz = 0;
    for(int b = 0; b < uCellLength.y; b++)
    {
        lx |= int((reversed >> uint(2 * b)) & 1u) << b;
        lz |= int((reversed >> uint(2 * b + 1)) & 1u) << b;
    }
    if(lx >= uCell.z || lz >= uCell.w)
    {
        // outside of the patch (edge cell): degenerate triangles, nothing is rasterized
        gl_Position = vec4(0.0, 0.0, -2.0, 1.0);
        vTexture = vec2(0.0);
        vColor = vec3(0.0);
        vTBN = mat3(1.0);
        vNormalBend = 0.0;
        return;
    }
    int ix = uCell.x + lx;
    int iz = uCell.y + lz;
    int id = iz * uNXNZ.x + ix;
    // rank of this plant inside its cell (0 = drawn longest)
    float importance = (float(gl_InstanceID) + 0.5) / float(uCellLength.x * uCellLength.x);

    float randomFromTextureOrg = textureLod(uNoiseMap, vec2((id % 512) / 512.0, (id % 512) / 512.0), 0.0).r;

    float randomFromTexture = (randomFromTextureOrg - 0.5) * 2.0;
    vec3 noiseNormalized = normalize(vec3(uNoise[id % 256].x, 0.01, uNoise[id % 256].y));

    vec3 axis = normalize(vec3(randomFromTexture * noiseNormalized.x, 0.001, randomFromTexture * noiseNormalized.z));
    float swayFactor = (sin((uPatchSizeTime.z + id) * randomFromTexture * noiseNormalized.x * noiseNormalized.z) + 1) * 0.5;
    float heightFactor2 = sqrt(sin(rand(noiseNormalized.xz) * noiseNormalized.x - noiseNormalized.y) + 1.5) * 0.6667;

    int n = int((randomFromTexture + 1.0) * 77);
    int n2 = int((randomFromTexture + 1.0) * 79);

    vec3 offsetXZ = vec3(
        uDXDZSwayRound.x * ix + uNoise[(id + n) % 256].x * randomFromTexture,
        0.0,
        uDXDZSwayRound.y * iz + uNoise[(id + n2) % 256].x * randomFromTexture
    );

    vec3 center = vec3(
        -uDXDZSwayRound.x * (uNXNZ.x - 1) / 2.0,
        0.0,
        -uDXDZSwayRound.y * (uNXNZ.y - 1) / 2.0
    );

    float radial = 1.0 - (
        length(offsetXZ + center) /
        length(-center - vec3((uPatchSizeTime.x + uPatchSizeTime.y) * 0.5, 0.0, (uPatchSizeTime.x + uPatchSizeTime.y) * 0.5))
    );
    float heightFactor = sqrt(clamp(radial, 0.0, 1.0));

    vec3 basePosition = uModelMatrix[3].xyz + center + offsetXZ;
    basePosition.y += getHeightFromTexture(basePosition);
    float cameraDistance = distance(uCameraPosition, basePosition);

    float keep = clamp(uLOD.x * heightFactor2 / max(cameraDistance, 0.0001), LOD_KEEP_MIN, 1.0);
    float widen = max(1.0 / (keep * (1.0 + 0.5 * LOD_KEEP_FADE)), 1.0);
    float lodScale = clamp((keep * (1.0 + LOD_KEEP_FADE) - importance) / (keep * LOD_KEEP_FADE), 0.0, 1.0);
    lodScale *= 1.0 - smoothstep(uLOD.y - uLOD.z, uLOD.y, cameraDistance);

    mat4 sMatrix = scaleMatrix(max(heightFactor, uDXDZSwayRound.w) * heightFactor2 * lodScale);

    mat3 rotMat =
        rotationMatrix(vec3(0, 1, 0), hashToUnitFloat(id) * (M_PI * 2.0)) *
        rotationMatrix(axis, swayFactor * (aPosition.y * M_PIHALF * uDXDZSwayRound.z));

    vec3 localPosition = vec3(aPosition.x * widen, aPosition.y, aPosition.z * widen);
    vec3 positionRandomized = (uModelMatrix * mat4(rotMat) * sMatrix * vec4(localPosition, 1.0)).xyz;
    vec4 totalLocalPos = vec4(positionRandomized + center + offsetXZ, 1.0);

    float heightFromTerrain = getHeightFromTexture(totalLocalPos.xyz);
    totalLocalPos.y += heightFromTerrain;

    mat4 normalRotation = uNormalMatrix * mat4(rotMat);
    vec3 totalNormal = (normalRotation * vec4(aNormal, 0.0)).xyz;
    vec3 totalTangent = (normalRotation * vec4(aTangent, 0.0)).xyz;
    vec3 totalBiTangent = (normalRotation * vec4(aBiTangent, 0.0)).xyz;

    gl_Position = uViewProjectionMatrix * totalLocalPos;

    vTexture = vec2(aTexture.x, 1.0 - aTexture.y);
    vTBN = mat3(normalize(totalTangent), normalize(totalBiTangent), normalize(totalNormal));
    vColor = min((pow(1.0 - aPosition.y, 3.0) * (-1.0) + 1.6) * vec3(1.0), vec3(1.0));
    vNormalBend = mix(NORMALBEND_NEAR, NORMALBEND_FAR, smoothstep(0.0, uLOD.y, cameraDistance));
}
