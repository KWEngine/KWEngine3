#version 400

layout(location = 0) out vec4 color;

in vec3 vBarycentric;

void main()
{
    float r = vBarycentric.x;
    float s = vBarycentric.y;
    float t = vBarycentric.z;

    float result = pow(s * 0.5 + t, 2);
    if(result < t)
    {
        //color = vec4(1.0); // WORKING
        color = vec4(1.0 / 255.0);
    }
    else
    {
        //discard; // WORKING
        color = vec4(0.0);
    }   
}