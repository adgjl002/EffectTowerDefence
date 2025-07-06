// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Custom/TextureMove"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

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
		Tags { 
			"Queue" = "Transparent"
			"RenderType"="Transparent" 
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
				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldPosition : TEXCOORD2;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			//UNITY_INSTANCING_CBUFFER_START(MyProperties)
			//	UNITY_DEFINE_INSTANCED_PROP(float, _GrayScale)
			//UNITY_INSTANCING_CBUFFER_END

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ClipRect;
			
			v2f vert (appdata v)
			{
				v2f o;

				//UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.worldPosition = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//UNITY_SETUP_INSTANCE_ID(i);
				//float grayScale = UNITY_ACCESS_INSTANCED_PROP(_GrayScale);

				fixed4 color = tex2D(_MainTex, float2(i.uv.x + _Time.x, i.uv.y));
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

				return color;
			}
			ENDCG
		}
	}
}
