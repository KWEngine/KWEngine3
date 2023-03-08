#version 400 core

layout(points) in;
layout(line_strip, max_vertices = 20) out;

uniform vec3 uRadius;
uniform mat4 uViewProjectionMatrix;

const float factor = 0.98765;

void main()
{
    // front
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();
    
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, -uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();
    
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, -uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();

    // back
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, -uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, -uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();

    // left lower line
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, -uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, -uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();

    // left upper line:
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * factor, uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();

    // right lower line
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, -uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, -uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();

    // right upper line:
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, uRadius.z * factor, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(uRadius.x * factor, uRadius.y * factor, -uRadius.z * factor, 0.0));
    EmitVertex();
    EndPrimitive();
}