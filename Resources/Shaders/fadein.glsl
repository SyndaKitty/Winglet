#version 120

varying vec2 fragTexCoord;
varying vec4 fragColor;

uniform sampler2D texture0;
uniform vec3 aaaaa;
uniform float t;

void main()
{
    float x = fragTexCoord.x - .5;
    float modifier = min(min(-abs(x)+2.*t-.2,6.-2.*t), 1.);
    vec4 texelColor = modifier * texture2D(texture0, fragTexCoord);
    gl_FragColor = texelColor * vec4(aaaaa, 1.);
}