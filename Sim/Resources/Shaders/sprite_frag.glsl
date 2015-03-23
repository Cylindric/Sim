//uniform sampler2D texture;
//uniform float pixel_threshold;
 
varying mediump vec2 TextureCoord;
varying mediump vec2 TextureSize;
uniform sampler2D texture;

void main()
{
	mediump vec2 realTexCoord = TextureCoord + (gl_PointCoord * TextureSize);
    mediump vec4 fragColor = texture2D(texture, realTexCoord);

	// Optional, emulate GL_ALPHA_TEST to use transparent images with
    // point sprites without worrying about z-order.
    // see: http://stackoverflow.com/a/5985195/806988
    if(fragColor.a == 0.0){
        discard;
    }

    //float factor = 1.0 / (pixel_threshold + 0.001);
	//vec2 pos = floor(gl_TexCoord[0].xy * factor + 0.5) / factor;
	//gl_FragColor = texture2D(texture, pos) * gl_Color;

	gl_FragColor = fragColor;
}