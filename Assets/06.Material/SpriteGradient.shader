Shader "Tavio/Sprites/Gradient" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask Tex", 2D) = "white" {}

		_StartColor("Start Color", Color) = (1, 1, 1, 1)
		_EndColor("End Color", Color) = (1, 1, 1, 1)

		_DirX("Gradient Dir X", Range(-1, 1)) = 0
		_DirY("Gradient Dir Y", Range(-1, 1)) = 0

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
	}
	SubShader{
		LOD 100
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True" }

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass{

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _MaskTex;
		fixed4 _StartColor;
		fixed4 _EndColor;
		fixed _DirX, _DirY;

		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
		};

		v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}

		fixed4 frag(v2f i) : COLOR
		{
			//fixed4 col = tex2D(_MainTex, i.uv);
			fixed4 mask = tex2D(_MaskTex, i.uv);
			fixed pos = (((i.uv.x * 2) -1) * _DirX) + (((i.uv.y * 2) - 1) *  _DirY);
			fixed4 result = lerp(_StartColor, _EndColor, smoothstep(-1,1,pos)) * mask;
			return result; //fixed4(result.rgb * mask.rgb, result.a * mask.a);
		}

		ENDCG
	}
	}
		FallBack "Diffuse"
}