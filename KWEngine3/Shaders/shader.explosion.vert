#version 400 core

#define M_PIHALF 3.141592 / 2.0
#define M_PI 3.141592
#define M_PIE 3.141592 * 2.0

const float TWO_PI = 6.2831853;
const float GOLDEN_ANGLE = 2.3999632;

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

out vec4 vPosition;
out vec3 vNormal;

uniform mat4 uViewProjectionMatrix;
uniform float uTime;
uniform vec3 uSpreadSizeLength;
uniform float uNumber;
uniform vec3 uPosition;
uniform int uAlgorithm;
uniform int uTowardsIndex;
uniform float uSeed;
uniform mat4 uDirectionMatrix;

mat4 rotationMatrix(vec3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;

    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

mat4 rotationMatrixToY = mat4(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1);

vec3 rotateVectorY(vec3 v, float angle)
{
    return vec3(cos(angle) * v.x - sin(angle) * v.z, v.y, sin(angle) * v.x + cos(angle) *v.z);
}

// rotates v around a normalized axis
vec3 rotateAroundAxis(vec3 v, vec3 axis, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
}

// integer hash (pcg), gives the same random numbers on every gpu
uint hashUint(uint v)
{
    uint state = v * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

// random number in [0, 1) for this particle; each stream is an independent sequence
float random01(uint stream)
{
    uint seed = hashUint(floatBitsToUint(uSeed) + stream * 2654435769u);
    uint h = hashUint(uint(gl_InstanceID) ^ seed);
    return float(h >> 8) * (1.0 / 16777216.0);
}

vec3 randomUnitVector(uint streamA, uint streamB)
{
    float y = random01(streamA) * 2.0 - 1.0;
    float a = random01(streamB) * TWO_PI;
    float r = sqrt(max(1.0 - y * y, 0.0));
    return vec3(r * cos(a), y, r * sin(a));
}

// evenly spread direction on the unit sphere (fibonacci sphere with some jitter, turned per explosion);
// capCos limits the directions to a cap around +Y (-1 = whole sphere, 0 = upper half)
vec3 sphereDirection(float index, float count, float capCos)
{
    float area = 1.0 - capCos;
    float y = 1.0 - area * (index + random01(0u)) / count;
    float r = sqrt(max(1.0 - y * y, 0.0));
    float phi = index * GOLDEN_ANGLE + fract(uSeed * 0.618034) * TWO_PI;
    vec3 d = vec3(r * cos(phi), y, r * sin(phi));
    vec3 jitter = vec3(random01(1u), random01(2u), random01(3u)) - 0.5;
    return normalize(d + jitter * 0.6 * sqrt(TWO_PI * area / count));
}

void main()
{
    float instancePercent = gl_InstanceID / (uNumber + 1);
    float sizeRand = random01(4u) * 0.9 + 0.1;

    mat4 orientation;
    vec3 lookAt;
    if(uTowardsIndex == 0) // regular explosion: directions evenly spread on a sphere, random orientation per particle
    {
        lookAt = sphereDirection(float(gl_InstanceID), uNumber, -1.0);
        orientation = rotationMatrix(randomUnitVector(5u, 6u), random01(7u) * TWO_PI);
    }
    else // ring explosion around Y or Z axis
    {
        vec3 ringAxis = uTowardsIndex == 1 ? vec3(0.0, 1.0, 0.0) : vec3(0.0, 0.0, 1.0);
        mat4 rotation = rotationMatrix(ringAxis, instancePercent * (1.95 * M_PI));
        lookAt = normalize(vec3(rotation[0][0], rotation[1][0], rotation[2][0]));
        orientation = uTowardsIndex == 1 ? rotationMatrixToY : mat4(1.0);
    }
    // axis that particles swirl around (stays inside the ring plane for ring explosions)
    vec3 spinAxis = uTowardsIndex == 2 ? vec3(0.0, 0.0, 1.0) : vec3(0.0, 1.0, 0.0);

    float sizeFactor = max(sin(2.0 * uTime + 1.25), 0.0);
    sizeFactor *= sizeFactor;
    sizeFactor *= sizeFactor;

    float t = clamp(uTime, 0.0, 1.0);
    float spread = uSpreadSizeLength.x;
    float size = uSpreadSizeLength.y;
    float len = uSpreadSizeLength.z;   // length of the direction vector, 0 = no direction (then uDirectionMatrix is the identity)

    float scale = sizeFactor * size * sizeRand;
    vec3 localPos = vec3(0.0);      // position relative to the explosion center, before uDirectionMatrix
    vec3 worldOffset = vec3(0.0);   // extra offset in world space (e.g. gravity)

    if(uAlgorithm == 0) // spread
    {
        vec3 dir = lookAt;
        if(len > 0.0 && uTowardsIndex == 0)
        {
            // cone around the direction: length 1 = half sphere, longer = narrower cone
            dir = sphereDirection(float(gl_InstanceID), uNumber, cos(M_PI / (1.0 + len)));
        }
        localPos = spread * uTime * dir * sizeRand;
    }
    else if(uAlgorithm == 1) // wind up
    {
        localPos.x = spread * uTime * lookAt.x * sizeRand + lookAt.x * sin(uTime * 1.5 * M_PI);
        localPos.y = spread * uTime * abs(lookAt.y) * sizeRand + len * uTime;
        localPos.z = spread * uTime * lookAt.z * sizeRand + lookAt.z * sin(uTime * 1.5 * M_PI);
    }
    else if(uAlgorithm == 2) // whirlwind up
    {
        localPos.x = 0.5 * spread * sin(uTime * M_PI) * (1.0 - lookAt.x) * sizeRand;
        localPos.y = spread * uTime * abs(lookAt.y) * sizeRand + len * uTime;
        localPos.z = 0.5 * spread * sin(uTime * M_PI) * (1.0 - lookAt.z) * sizeRand;
        // swirl speed per particle: random for regular explosions (the instance id is tied to the direction there)
        float swirlSpeed = uTowardsIndex == 0 ? random01(11u) : instancePercent;
        localPos = rotateVectorY(localPos, swirlSpeed * uTime * M_PIE * 2.0);
    }
    else if(uAlgorithm == 3) // exhaust
    {
        localPos.x = spread * lookAt.x * sizeRand - lookAt.x * 0.5 * uTime;
        localPos.y = uTime * abs(lookAt.y) * sizeRand + len * uTime;
        localPos.z = spread * lookAt.z * sizeRand - lookAt.z * 0.5 * uTime;
    }
    else if(uAlgorithm == 5) // implode: particles get sucked into the center while swirling, then pop
    {
        const float T_IMPLODE = 0.8;
        float startRadius = spread * (0.6 + 0.4 * random01(8u));
        float p = min(t / T_IMPLODE, 1.0);
        float p2 = p * p;
        vec3 dir = rotateAroundAxis(lookAt, spinAxis, p2 * 0.75 * TWO_PI);
        if(t < T_IMPLODE)
        {
            localPos = dir * startRadius * (1.0 - p2);
            scale = size * sizeRand * (1.0 - 0.75 * p2) * smoothstep(0.0, 0.1, t);
        }
        else if(len > 0.0)
        {
            // the collected ball is shot along the direction, particles trail behind like a comet tail
            float q = (t - T_IMPLODE) / (1.0 - T_IMPLODE);
            float lag = random01(9u) * 0.35;
            float travel = clamp((q - lag) / (1.0 - lag), 0.0, 1.0);
            localPos = vec3(0.0, len * travel, 0.0) + dir * spread * 0.12 * sizeRand;
            scale = size * sizeRand * (0.25 + 0.6 * sin(q * 0.5 * M_PI)) * (1.0 - q * q);
        }
        else
        {
            float q = (t - T_IMPLODE) / (1.0 - T_IMPLODE);
            localPos = dir * spread * 0.6 * sizeRand * sqrt(q);
            scale = size * sizeRand * (0.25 + 1.5 * sin(q * M_PI)) * (1.0 - q);
        }
    }
    else if(uAlgorithm == 6) // firework: rocket flies along the direction, then bursts into a ball and sinks down
    {
        float launch = len > 0.0 ? 0.3 : 0.0;   // without a direction there is no rocket, the ball bursts right away
        vec3 apex = vec3(0.0, len, 0.0);
        if(t < launch)
        {
            float lag = random01(8u) * 0.3;
            float p = clamp((t / launch - lag) / (1.0 - lag), 0.0, 1.0);
            localPos = apex * (1.0 - (1.0 - p) * (1.0 - p)) + lookAt * spread * 0.03;
            scale = size * 0.5 * sizeRand * smoothstep(0.0, 0.05, t);
        }
        else
        {
            float q = (t - launch) / (1.0 - launch);
            float burst = 1.0 - (1.0 - q) * (1.0 - q) * (1.0 - q);
            localPos = apex + lookAt * spread * (0.8 + 0.2 * sizeRand) * burst;
            worldOffset.y = -0.6 * spread * q * q;
            float flicker = 0.75 + 0.25 * sin(q * 40.0 + random01(9u) * TWO_PI);
            scale = size * (0.6 + 0.4 * sizeRand) * (1.0 - q * q) * flicker;
        }
    }
    else if(uAlgorithm == 7) // fountain: steady stream of particles that shoot along the direction and fall back down
    {
        const float LIFE = 0.45;
        float birth = random01(8u) * (1.0 - LIFE);
        float age = (t - birth) / LIFE;
        float alive = step(0.0, age) * step(age, 1.0);
        age = clamp(age, 0.0, 1.0);

        // jet axis is the direction (local y): its length sets the height (without a direction: the radius),
        // the radius sets how far the drops land
        vec3 side = vec3(lookAt.x, 0.0, lookAt.z);
        float sideLength = length(side);
        side = sideLength > 0.0001 ? side / sideLength : vec3(1.0, 0.0, 0.0);

        float height = (len > 0.0 ? len : spread) * (0.75 + 0.25 * random01(9u));
        vec3 velocity = vec3(0.0, 4.0 * height, 0.0) + side * spread * sqrt(random01(10u));
        localPos = velocity * age;
        worldOffset.y = -4.0 * height * age * age;
        scale = size * sizeRand * smoothstep(0.0, 0.15, age) * (1.0 - 0.5 * age) * alive;
    }
    else if(uAlgorithm == 8) // rain: particles flutter down from an area around the center (money, hearts, confetti ...)
    {
        const float LIFE = 0.35;
        float birth = random01(8u) * (1.0 - LIFE);
        float age = (t - birth) / LIFE;
        float alive = step(0.0, age) * step(age, 1.0);
        age = clamp(age, 0.0, 1.0);

        // start on a horizontal disc (sunflower pattern), or on a circle for ring explosions
        float angle;
        float radius;
        if(uTowardsIndex == 0)
        {
            angle = float(gl_InstanceID) * GOLDEN_ANGLE + fract(uSeed * 0.618034) * TWO_PI;
            radius = sqrt((float(gl_InstanceID) + random01(9u)) / uNumber);
        }
        else
        {
            angle = float(gl_InstanceID) / uNumber * TWO_PI;
            radius = 1.0;
        }
        float phase = random01(10u) * TWO_PI;
        float swing = spread * 0.08 * (0.5 + sizeRand);
        vec3 start = vec3(cos(angle), 0.0, sin(angle)) * radius * spread;
        vec3 flutter = vec3(sin(age * 5.0 * M_PI + phase), 0.0, cos(age * 3.7 * M_PI + phase) * 0.6) * swing;

        // the start area stays horizontal, the particles fall along the direction by its length
        // (without a direction: straight down by the radius)
        float fall = len > 0.0 ? len : -spread;
        localPos = vec3(0.0, fall * age, 0.0);
        worldOffset = start + flutter;

        float spin = (1.0 + 2.0 * random01(11u)) * TWO_PI;
        orientation = rotationMatrix(randomUnitVector(12u, 13u), age * spin) * orientation;
        scale = size * sizeRand * smoothstep(0.0, 0.08, age) * (1.0 - smoothstep(0.85, 1.0, age)) * alive;
    }
    else // spark
    {
        float downfactor = smoothstep(0.1, 1.0, uTime);
        localPos = spread * uTime * lookAt * sizeRand + vec3(0.0, len * uTime, 0.0);
        worldOffset.y = -0.5 * spread * downfactor;
    }

    mat4 modelMatrix = orientation;
    modelMatrix[0].xyz *= scale;
    modelMatrix[1].xyz *= scale;
    modelMatrix[2].xyz *= scale;
    modelMatrix[3] = vec4(localPos, 1.0);
    modelMatrix = uDirectionMatrix * modelMatrix;
    modelMatrix[3].xyz += uPosition + worldOffset;

    mat4 mvp = uViewProjectionMatrix * modelMatrix;
    vPosition = modelMatrix * vec4(aPosition, 1.0);
    vNormal = (modelMatrix * vec4(aNormal, 0.0)).xyz;
	gl_Position = mvp * vec4(aPosition, 1.0);
}
