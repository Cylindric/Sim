uniform sampler2D texture;
uniform float pixel_threshold;
uniform vec2 TextureSize;
 
void main()
{
	vec2 pos = vec2(gl_TexCoord[0].x * TextureSize.x, gl_TexCoord[0].y * TextureSize.y);
	gl_FragColor = texture2D(texture, pos) * gl_Color;	
}