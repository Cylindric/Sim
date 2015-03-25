uniform sampler2D texture;
uniform vec2 TextureSize;
uniform vec2 TextureOffset;

void main()
{
	vec2 pos = vec2(gl_TexCoord[0].xy * TextureSize.xy) + TextureOffset;
	gl_FragColor = texture2D(texture, pos) * gl_Color;	
}