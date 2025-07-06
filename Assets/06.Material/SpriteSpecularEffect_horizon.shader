Shader "UI/Unlit/SpriteSpecularEffect_Horizon"
{
	Properties
	{
		[PerRendererData] _MainTex("Main Tex", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_BlendTex("Blend Tex", 2D) = "white" {}
		_BlendMaskTex("Blend Mask Tex", 2D) = "white" {}

		_EffectSpeed("Effect Speed", Range(-100,100)) = 40
		_AddtiveAmount("Addtive Amount", Range(0,1)) = 0.4

		// required for UI.Mask
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0			
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
	    _ScrollXSpeed("X Scroll Speed", Float) = 2
		_ScrollYSpeed("Y Scroll Speed", Float) = 2
		_ColorMask("Color Mask", Float) = 15
	}
		SubShader
	{
		LOD 100

		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True" 
		}

		// required for UI.Mask
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

		Pass
	{


		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
//#pragma multi_compile_instancing

#include "UnityCG.cginc"
#include "UnityUI.cginc"

#pragma multi_compile __ UNITY_UI_CLIP_RECT

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		fixed4 color    : COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float2 blenduv : TEXCOORD1;
		float4 vertex : SV_POSITION;
		float4 worldPosition : TEXCOORD2;
		fixed4 color    : COLOR;
	};

	fixed4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _BlendTex;
	sampler2D _BlendMaskTex;
	fixed _AddtiveAmount;
	fixed _EffectSpeed;
	float4 _ClipRect;
	float _ScrollXSpeed;
        float _ScrollYSpeed;

	v2f vert(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);

		o.worldPosition = v.vertex;
		o.vertex = UnityObjectToClipPos(v.vertex);

		o.uv = TRANSFORM_TEX(v.uv, _MainTex);

		o.blenduv = o.uv;
		o.blenduv.x += frac(float2(_ScrollXSpeed, _ScrollYSpeed) * _Time.xx);

		o.color = v.color * _Color;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv) * i.color;
		fixed4 spec = tex2D(_BlendTex, i.blenduv);
		fixed mask = tex2D(_BlendMaskTex, i.uv);
		//col.rgb = mask;
		col.rgb += (spec * spec.a * mask) * _AddtiveAmount;
		
#ifdef UNITY_UI_CLIP_RECT
		col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
#endif

		return col;
	}
		ENDCG
	}
	}
}
