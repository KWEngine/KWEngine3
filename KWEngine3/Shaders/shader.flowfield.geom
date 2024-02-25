#version 400 core

layout(points) in;
layout(line_strip, max_vertices = 20) out;

uniform vec2 uRadius;
uniform mat4 uViewProjectionMatrix;

const float f = 0.9;

void main()
{

    // front
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, +uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();
    
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, -uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();
    
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, -uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();

    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();
    EndPrimitive();

   
    // back
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, -uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, -uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, +uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
    EndPrimitive();                                                            
                                                                                                                                                              
    // left lower line                                                         
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, -uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, -uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();                                                              
    EndPrimitive();                                                            
                                                                               
    // left upper line:                                                        
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, +uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(-uRadius.x * f, +uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
    EndPrimitive();                                                            
                                                                               
    // right lower line                                                        
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, -uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, -uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();                                                              
    EndPrimitive();                                                            
                                                                               
    // right upper line:                                                       
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, +uRadius.x * f, 0.0));
    EmitVertex();                                                              
                                                                               
    gl_Position = uViewProjectionMatrix * (gl_in[0].gl_Position + vec4(+uRadius.x * f, +uRadius.y, -uRadius.x * f, 0.0));
    EmitVertex();
    EndPrimitive();
    
}