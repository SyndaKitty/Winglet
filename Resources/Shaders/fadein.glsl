#version 120

varying vec2 fragTexCoord;
varying vec4 fragColor;

uniform sampler2D texture0;
uniform float t;

void main()
{
    float x = fragTexCoord.x - .5;
    float modifier = min(min(-abs(x)+t-.2,3.5-t), 1.);
    vec4 texelColor = modifier * texture2D(texture0, fragTexCoord);
    gl_FragColor = texelColor;
}