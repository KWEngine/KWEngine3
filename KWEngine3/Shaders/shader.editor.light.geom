#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 256) out; 

uniform mat4 uViewProjectionMatrix;
uniform mat4 uViewProjectionMatrixLight;
uniform vec3 uPosition;
uniform vec4 uNearFarFOVType;
uniform vec3 uLookAtVector;

const vec3 front = vec3(+0,+0,+1);
const vec3 back  = vec3(+0,+0,-1);
const vec3 left  = vec3(-1,+0,+0);
const vec3 right = vec3(+1,+0,+0);
const vec3 leftback  = vec3(-0.707,+0,-0.707);
const vec3 rightback = vec3(+0.707,+0,-0.707);
const vec3 leftfront  = vec3(-0.707,+0,+0.707);
const vec3 rightfront = vec3(+0.707,+0,+0.707);

const vec4 clipSpaceFarTopLeft     = vec4(-1,1,1,1);
const vec4 clipSpaceFarTopRight    = vec4(+1,1,1,1);
const vec4 clipSpaceFarBottomLeft  = vec4(-1,-1,1,1);
const vec4 clipSpaceFarBottomRight = vec4(+1,-1,1,1);
const vec4 clipSpaceNearTopLeft     = vec4(-1,1,-1,1);
const vec4 clipSpaceNearTopRight    = vec4(+1,1,-1,1);
const vec4 clipSpaceNearBottomLeft  = vec4(-1,-1,-1,1);
const vec4 clipSpaceNearBottomRight = vec4(+1,-1,-1,1);

const float yOffset1 = 0.5;
const float yOffset2 = 0.975;

void main()
{
	
	
	// point
	if(uNearFarFOVType.w == 0.0)
	{
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + right * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + back * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + left * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		// upper middle circle
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + right * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightback * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();

		// ======================

		//connect center and upper middle:
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.y, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + right * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + right * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightback * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + back * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + left * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		// =====================
		// connect lower circle with middle:
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + right * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + right * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + back * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftback * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + left * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftfront * uNearFarFOVType.y, 1.0);
		EmitVertex();
		EndPrimitive();

		
		// ===============
		// connect lower circle with bottom
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + rightfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + right * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + right * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + rightback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + back * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + leftback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + left * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + leftfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();

		
		// ===============
		//connect upper circle with top:
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + rightfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + right * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + right * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + rightback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + rightback * uNearFarFOVType.y *yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + back * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + leftback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + left * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + leftfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();
		


		// ======================

		// lower middle circle
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + right * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + rightback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + back * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftback * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + left * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + leftfront * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset1, 0) + front * uNearFarFOVType.y * yOffset1, 1.0);
		EmitVertex();
		EndPrimitive();

		// upper circle
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + rightfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + right * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + rightback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + back * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + leftback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + left * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + leftfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();

		// lower circle
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + rightfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + right * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + rightback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + back * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + leftback * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + left * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + leftfront * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + vec3(0, uNearFarFOVType.y * -yOffset2, 0) + front * uNearFarFOVType.y * (1.0 - yOffset2), 1.0);
		EmitVertex();
		EndPrimitive();

		// middle inner circle
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightfront * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + right * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + rightback * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + back * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftback * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + left * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + leftfront * uNearFarFOVType.x, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * vec4(uPosition + front * uNearFarFOVType.x, 1.0);
		EmitVertex();
		EndPrimitive();

		
	}
	else // sun and directional/spot
	{
		mat4 inverseVPLight = inverse(uViewProjectionMatrixLight);
		float inverseW = 1.0; //(inverseVPLight * vec4(0,0,0,1)).w;
		
		// outer circle
		vec4 farBottomLeft = inverseVPLight * (clipSpaceFarBottomLeft * inverseW);
		vec4 farBottomRight = inverseVPLight * (clipSpaceFarBottomRight* inverseW);
		vec4 farTopLeft = inverseVPLight * (clipSpaceFarTopLeft* inverseW);
		vec4 farTopRight = inverseVPLight * (clipSpaceFarTopRight* inverseW);
		vec4 nearBottomLeft = inverseVPLight * (clipSpaceNearBottomLeft * inverseW);
		vec4 nearBottomRight = inverseVPLight * (clipSpaceNearBottomRight * inverseW);
		vec4 nearTopLeft = inverseVPLight * (clipSpaceNearTopLeft * inverseW);
		vec4 nearTopRight = inverseVPLight * (clipSpaceNearTopRight * inverseW);

		// connection to near plane:
		gl_Position = uViewProjectionMatrix * vec4(uPosition, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearBottomLeft;
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearBottomRight;
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearTopLeft;
		EmitVertex();
		EndPrimitive();
		gl_Position = uViewProjectionMatrix * vec4(uPosition, 1.0);
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearTopRight;
		EmitVertex();
		EndPrimitive();

		// Far rect
		gl_Position = uViewProjectionMatrix * farBottomLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farBottomRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farTopRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farTopLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farBottomLeft;
		EmitVertex();
		EndPrimitive();

		// Near rect
		gl_Position = uViewProjectionMatrix * nearBottomLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearBottomRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearTopRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearTopLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * nearBottomLeft;
		EmitVertex();
		EndPrimitive();

		// edges
		gl_Position = uViewProjectionMatrix * nearBottomLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farBottomLeft;
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * nearBottomRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farBottomRight;
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * nearTopLeft;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farTopLeft;
		EmitVertex();
		EndPrimitive();

		gl_Position = uViewProjectionMatrix * nearTopRight;
		EmitVertex();
		gl_Position = uViewProjectionMatrix * farTopRight;
		EmitVertex();
		EndPrimitive();
	}
}