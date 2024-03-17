#version 400 core

layout (points) in;
layout (line_strip, max_vertices = 30) out; 

uniform mat4 uModelViewProjection;
uniform int uType;
uniform vec3 uCenterOfMass;
uniform vec3 uDimensions;
uniform vec3 uLookAtVector;
uniform vec3 uLookAtVectorTop;
uniform vec3 uLookAtVectorRight;

void main()
{
    vec4 p = vec4(uCenterOfMass,1);
    if(uType > 0)
    {
        // hinten unten x
	    gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();

        // vorne unten x
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();

        // hinten oben x
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();

        // vorne oben x
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        

        // rechts oben
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        // rechts unten
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();

        // links oben
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        // links unten
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();

        // vertikal links vorne
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        // vertikal rechts vorne
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, +uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        // vertikal links hinten
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(-uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
        // vertikal rechts hinten
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, -uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        gl_Position = uModelViewProjection * (p + vec4(+uDimensions.x / 2, +uDimensions.y / 2, -uDimensions.z / 2, 0.0)); 
        EmitVertex();
        EndPrimitive();
    }
    
    gl_Position = uModelViewProjection * (p - vec4(uLookAtVector, 0.0) * length(uLookAtVector) * 0.05);
    EmitVertex();
    gl_Position = uModelViewProjection * (p + vec4(uLookAtVector, 0.0) * length(uLookAtVector) * 0.05);
    EmitVertex();
    EndPrimitive();
    gl_Position = uModelViewProjection * (p - vec4(uLookAtVectorRight, 0.0) * length(uLookAtVectorRight) * 0.05);
    EmitVertex();
    gl_Position = uModelViewProjection * (p + vec4(uLookAtVectorRight, 0.0) * length(uLookAtVectorRight) * 0.05);
    EmitVertex();
    EndPrimitive();
    gl_Position = uModelViewProjection * (p - vec4(uLookAtVectorTop, 0.0) * length(uLookAtVectorTop) * 0.05);
    EmitVertex();
    gl_Position = uModelViewProjection * (p + vec4(uLookAtVectorTop, 0.0) * length(uLookAtVectorTop) * 0.05);
    EmitVertex();
    EndPrimitive();
    

    //LAV
	float w = (uModelViewProjection * vec4(0,0,0,1)).w;
    gl_Position = uModelViewProjection * ((p + vec4(uLookAtVector, 0.0)) * w); 
    EmitVertex();
    gl_Position = uModelViewProjection * ((p + vec4(uLookAtVector * 10, 0.0)) * w);
    EmitVertex();
    EndPrimitive();
}