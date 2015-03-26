uniform sampler2D texture;
uniform vec2 TextureSize;
uniform vec2 TextureOffset;
uniform vec4 Colour;

void main()
{
	vec2 pos = vec2(gl_TexCoord[0].xy * TextureSize.xy) + TextureOffset;
	vec4 col = texture2D(texture, pos) * gl_Color;	
	
	// tint
	col = col + Colour * col.a;
	
	gl_FragColor = col;	
}