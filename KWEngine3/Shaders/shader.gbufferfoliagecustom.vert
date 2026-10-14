#version 400 core

// G-buffer pass for FoliageObjectCustom instances (alpha tested imposter cards).

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexture;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in	vec3 aTangent;
layout(location = 4) in	vec3 aBiTangent;

uniform mat4 uViewProjectionMatrix;
uniform vec3 uPatchSizeTime;
uniform vec2 uNoise[256];
uniform vec3 uTerrainScale;
uniform vec3 uTerrainPosition;
uniform sampler2D uTerrainHeightMap;
uniform vec4 uPosScaleUV1[256];
uniform vec4 uPosScaleUV2[256];
uniform vec3 uCameraPosition;
uniform vec2 uFade; // x = distance where fading starts, y = draw distance (instances have shrunk to zero there)

out vec3 vImposterCenter;
out vec4 vPosition;
out vec2 vTexture;
out vec3 vColor;

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

        // red channel like the terrain shader (gray height maps are single channel textures)
        height = textureLod(uTerrainHeightMap, vec2(uvX, uvZ), 0.0).r * uTerrainScale.y + uTerrainPosition.y;
    }
    return height;
}

vec3 computeImposterBackBend(vec3 imposterCenterWorld, float height01, float plantHeight)
{
    vec3 toCamera = normalize(uCameraPosition - imposterCenterWorld);

    vec3 bendDir = vec3(-toCamera.x, 0.0, -toCamera.z);
    float bendLen = length(bendDir);

    if (bendLen > 0.0001)
        bendDir /= bendLen;
    else
        bendDir = vec3(0.0, 0.0, 0.0);

    float topMask = clamp(height01, 0.0, 1.0);
    topMask = topMask * topMask;
    topMask = topMask * topMask;

    float cameraAboveFactor = smoothstep(0.15, 0.85, max(toCamera.y, 0.0));

    float strength = 0.33;
    float bendAmount = strength * plantHeight * topMask * cameraAboveFactor;

    return bendDir * bendAmount;
}

void main()
{
    vec3 positionFromUBO = uPosScaleUV1[gl_InstanceID].xyz;
    vec3 scaleFromUBO = vec3(uPosScaleUV1[gl_InstanceID].w, uPosScaleUV2[gl_InstanceID].x, uPosScaleUV2[gl_InstanceID].y);

    vec3 noiseNormalized = normalize(vec3(
        uNoise[gl_InstanceID % 256].x,
        0.01,
        uNoise[gl_InstanceID % 256].y
    ));

    float swayFactor =
        (sin((uPatchSizeTime.z + (gl_InstanceID % 256) * noiseNormalized.x) *
        noiseNormalized.x * noiseNormalized.z) + 1.0) * 0.02;

    vec3 aPosition2 = vec3(
        aPosition.x + aPosition.x * sin(uPatchSizeTime.z + (gl_InstanceID * 0.33) * noiseNormalized.x) * swayFactor * aPosition.y,
        aPosition.y,
        aPosition.z + aPosition.z * sin(uPatchSizeTime.z + (gl_InstanceID * 0.33) * noiseNormalized.x) * swayFactor * aPosition.y
    );

    vec3 imposterBaseCenter = positionFromUBO;
    float heightFromTerrain = getHeightFromTexture(imposterBaseCenter);
    imposterBaseCenter.y += heightFromTerrain;

    // shrink the instance to zero towards the draw distance instead of popping
    float fadeScale = 1.0 - smoothstep(uFade.x, uFade.y, distance(uCameraPosition, imposterBaseCenter));
    scaleFromUBO *= fadeScale;

    vec3 localPos = aPosition2 * scaleFromUBO;
    float height01 = clamp(aPosition.y, 0.0, 1.0);
    vec3 backBendOffset = computeImposterBackBend(imposterBaseCenter, height01, scaleFromUBO.y);

    vec3 worldPos = localPos + positionFromUBO;
    worldPos.y += heightFromTerrain;
    worldPos += backBendOffset;

    vPosition = vec4(worldPos, 1.0);
    vImposterCenter = imposterBaseCenter + vec3(0.0, scaleFromUBO.y * 0.35, 0.0);
    vTexture = aTexture;
    vColor = min((pow(1.0 - aPosition.y, 3.0) * (-1.0) + 1.6) * vec3(1.0), vec3(1.0));

    gl_Position = uViewProjectionMatrix * vPosition;
}
