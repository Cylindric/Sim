#version 150

/*

uniform mat4 Projection;
uniform mat4 Modelview;
// The radius of the point in OpenGL units, eg: "20.0"
uniform float PointSize;
// The size of the sprite being rendered. My sprites are square
// so I'm just passing in a float.  For non-square sprites pass in
// the width and height as a vec2.
uniform float TextureCoordPointSize;

attribute vec4 Position;
attribute vec4 ObjectCenter;
// The top left corner of a given sprite in the sprite-sheet
uniform vec2 TextureCoordIn;

varying vec2 TextureCoord;
varying vec2 TextureSize;

void main(void)
{
    gl_Position = Projection * Modelview * Position;
    TextureCoord = TextureCoordIn;
    TextureSize = vec2(TextureCoordPointSize, TextureCoordPointSize);

    // This is optional, it is a quick and dirty way to make the points stay the same
    // size on the screen regardless of distance.
    gl_PointSize = PointSize / Position.w;
}

*/

//in vec3 vert;
//in vec2 vertTexCoord;
//out vec2 fragTexCoord;

void main() 
{
	//fragTexCoord = vertTexCoord;
	//gl_Position = vec4(vert, 1);
}
