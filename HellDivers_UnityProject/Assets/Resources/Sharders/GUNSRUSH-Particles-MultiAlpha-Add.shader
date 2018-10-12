// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Particles Multi Alpha v3 edited by Fred 2012.12.19
// fixed some bugs
// v3 - set 'Queue' to 'Transparent+100'

Shader "GodGame/Particles/Multi Alpha ~Addtive" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaTex ("AlphaMask", 2D) = "white" { }
	}
	
	Category {
		Tags { "Queue"="Transparent+100" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		AlphaTest Greater .01
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
		SubShader {
			Pass {
			
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles
	
				#include "UnityCG.cginc"
	
				sampler2D _MainTex;
				sampler2D _AlphaTex;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
	
				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
				};
				
				float4 _MainTex_ST;
				float4 _AlphaTex_ST;
	
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
					o.uv2 = TRANSFORM_TEX(v.texcoord,_AlphaTex);
					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv);
					tex.a *= tex2D(_AlphaTex, i.uv2).r;
					
					return 2.0f * i.color * _TintColor * tex;
				}
				ENDCG 
			}
		} 	
	}
	Fallback "VertexLit"
}
