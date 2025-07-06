Shader "Tavio/Sprites/RadialAlphaGradient"
{
    Properties
    {

       _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}

        _ColorCount("Color Count", int) = 3
		_AlphaCount("Alpha Count", int) = 3
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
          LOD 100
            Cull Off
            Lighting Off
            ZWrite Off

            Fog { Mode Off }
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"
              struct appdata_t {
                 float4 vertex : POSITION;
                 float2 texcoord : TEXCOORD0;
                 float4 color    : COLOR;
             };

            struct v2f {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half4 color : COLOR;
            };

            fixed4 _Color;
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = _Color;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            uniform fixed4 _Colors[32];
            uniform fixed _ColorAnchors[32];
            fixed4 _CurrentColor;
            fixed4 _NextColor;
            int _ColorCount = 0;

            uniform fixed _Alphas[32];
            uniform fixed _AlphaAnchors[32];
            fixed _CurrentAlpha;
            fixed _NextAlpha;
            int _AlphaCount = 0;


            fixed4 frag (v2f i) : SV_Target
            {
                
                float pos = 1 - length(i.texcoord - float2(0.5, 0.5)) * 1.41421356237; // 1.141... = sqrt(2)

                fixed colorAnchor;
                fixed nextColorAnchor;

                for (int j = 0; j < _ColorCount; j++) {
                    colorAnchor = _ColorAnchors[j];
                    nextColorAnchor = _ColorAnchors[j + 1];
                    if (pos >= colorAnchor && pos < nextColorAnchor) {
                        _CurrentColor = _Colors[j];
                        _NextColor = _Colors[j + 1];
                        j = _ColorCount;
                    }
                }
                
                fixed alphaAnchor;
                fixed nextAlphaAnchor;

                for (int j = 0; j < _AlphaCount; j++) {
                    alphaAnchor = _AlphaAnchors[j];
                    nextAlphaAnchor = _AlphaAnchors[j + 1];
                    if (pos >= alphaAnchor && pos < nextAlphaAnchor) {
                        _CurrentAlpha = _Alphas[j];
                        _NextAlpha = _Alphas[j + 1];
                        j = _AlphaCount;
                    }
                }

    			fixed4 result = lerp(_CurrentColor, _NextColor, smoothstep(colorAnchor, nextColorAnchor, pos));
            
	    		result.w = lerp(_CurrentAlpha, _NextAlpha, smoothstep(alphaAnchor, nextAlphaAnchor, pos));

		    	return result;

            }

        ENDCG
        }
    }
}