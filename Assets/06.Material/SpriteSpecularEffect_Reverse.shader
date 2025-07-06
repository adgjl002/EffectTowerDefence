 Shader "UI/Unlit/SpriteSpecularEffect_Reverse"
{
	Properties
	{
		[PerRendererData] _MainTex("Main Tex", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_BlendTex("Blend Tex", 2D) = "white" {}
		_EffectSpeed("Effect Speed", Range(-100,100)) = 40
		_AddtiveAmount("Addtive Amount", Range(0,1)) = 0.4

		// required for UI.Mask
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0			
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
	
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
		fixed4 color    : COLOR;
	};

	fixed4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _BlendTex;
	fixed _AddtiveAmount;
	fixed _EffectSpeed;

	v2f vert(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);

		o.vertex = UnityObjectToClipPos(v.vertex);

		o.uv = TRANSFORM_TEX(v.uv, _MainTex);

		o.blenduv = o.uv;
		o.blenduv.y -= tan(_Time * _EffectSpeed);

		o.color = v.color * _Color;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv) * i.color;
		fixed4 spec = tex2D(_BlendTex, i.blenduv);

		col.rgb += (spec * spec.a) * _AddtiveAmount;
		return col;
	}
		ENDCG
	}
	}
}
