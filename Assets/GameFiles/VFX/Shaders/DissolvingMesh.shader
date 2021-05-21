// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DissolvingMesh"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_TextureDissolve("TextureDissolve", 2D) = "white" {}
		_AlphaThreshold("Alpha Threshold", Range( 0 , 1)) = 0
		[Toggle(_PARTICLECONTROLSALPHA_ON)] _ParticleControlsAlpha("ParticleControlsAlpha", Float) = 0
		[Toggle(_FRESNEL_ON)] _Fresnel("Fresnel", Float) = 0
		_FresnelPower("FresnelPower", Float) = 5
		_FresnelColor("Fresnel Color", Color) = (1,0.9969,0.8160377,0)
		_FresnelScale("FresnelScale", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Front
			Lighting Off 
			ZWrite Off
			ZTest Always
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _FRESNEL_ON
				#pragma shader_feature_local _PARTICLECONTROLSALPHA_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float _FresnelScale;
				uniform float _FresnelPower;
				uniform float4 _FresnelColor;
				uniform float _AlphaThreshold;
				uniform sampler2D _TextureDissolve;
				uniform float4 _TextureDissolve_ST;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord4.xyz = ase_worldNormal;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float3 appendResult22 = (float3(i.color.rgb));
					float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
					float4 temp_output_21_0 = ( float4( appendResult22 , 0.0 ) * tex2DNode1 );
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord4.xyz;
					float fresnelNdotV23 = dot( ase_worldNormal, ase_worldViewDir );
					float fresnelNode23 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV23, _FresnelPower ) );
					#ifdef _FRESNEL_ON
					float4 staticSwitch26 = ( fresnelNode23 * _FresnelColor * temp_output_21_0 );
					#else
					float4 staticSwitch26 = temp_output_21_0;
					#endif
					#ifdef _PARTICLECONTROLSALPHA_ON
					float staticSwitch15 = i.color.a;
					#else
					float staticSwitch15 = _AlphaThreshold;
					#endif
					float2 uv_TextureDissolve = i.texcoord.xy * _TextureDissolve_ST.xy + _TextureDissolve_ST.zw;
					float4 tex2DNode9 = tex2D( _TextureDissolve, uv_TextureDissolve );
					float ifLocalVar12 = 0;
					if( staticSwitch15 <= tex2DNode9.r )
					ifLocalVar12 = 0.0;
					else
					ifLocalVar12 = tex2DNode1.a;
					float4 appendResult7 = (float4(staticSwitch26.rgb , ifLocalVar12));
					

					fixed4 col = appendResult7;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
2253;72;1178;954;680.6477;738.1487;1.350737;True;False
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;8;-1213.84,7.129322;Inherit;True;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;6;-1175.641,-306.725;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;22;-828.5521,-289.7798;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-661.7367,-443.6877;Inherit;False;Property;_FresnelPower;FresnelPower;5;0;Create;True;0;0;0;False;0;False;5;2.14;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-667.1398,-531.4856;Inherit;False;Property;_FresnelScale;FresnelScale;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-948,8;Inherit;True;Property;_MainTexture;Main Texture;1;0;Create;True;0;0;0;False;0;False;-1;3ebdd3cdb2a40bc41a6455906dcb394c;3ebdd3cdb2a40bc41a6455906dcb394c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-1285.172,-142.1164;Inherit;False;Property;_AlphaThreshold;Alpha Threshold;1;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-424.7902,-124.5192;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;27;-507.7527,-369.3972;Inherit;False;Property;_FresnelColor;Fresnel Color;6;0;Create;True;0;0;0;False;0;False;1,0.9969,0.8160377,0;1,0.9969,0.8160377,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;23;-457.7758,-557.1499;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-637.4232,214.0287;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1000.928,216.9166;Inherit;True;Property;_TextureDissolve;TextureDissolve;0;0;Create;True;0;0;0;False;0;False;-1;424dd4eeb22d2a84e8838d9c048d9f99;14152e439455785449bf424518a5816e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-152.5087,-353.1883;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;15;-966.0495,-145.8951;Inherit;False;Property;_ParticleControlsAlpha;ParticleControlsAlpha;2;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;26;23.08699,-339.6808;Inherit;False;Property;_Fresnel;Fresnel;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;12;-410.5148,68.49415;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;16;-197.6914,234.3723;Inherit;False;Property;_LerpMethod;LerpMethod;3;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-572.0723,292.1747;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;-392.9928,338.7647;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-662.3398,382.4424;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;7;291.1202,-2.110179;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;459.4607,-14.66401;Float;False;True;-1;2;ASEMaterialInspector;0;7;DissolvingMesh;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;True;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;True;True;1;False;-1;False;True;True;True;True;False;0;False;-1;False;False;False;False;False;False;False;False;True;True;2;False;-1;True;7;False;-1;False;True;4;Queue=Transparent=Queue=1;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;22;0;6;0
WireConnection;1;0;8;0
WireConnection;21;0;22;0
WireConnection;21;1;1;0
WireConnection;23;2;32;0
WireConnection;23;3;29;0
WireConnection;24;0;23;0
WireConnection;24;1;27;0
WireConnection;24;2;21;0
WireConnection;15;1;14;0
WireConnection;15;0;6;4
WireConnection;26;1;21;0
WireConnection;26;0;24;0
WireConnection;12;0;15;0
WireConnection;12;1;9;1
WireConnection;12;2;1;4
WireConnection;12;3;13;0
WireConnection;12;4;13;0
WireConnection;16;1;12;0
WireConnection;16;0;17;0
WireConnection;17;0;18;0
WireConnection;17;1;1;4
WireConnection;17;2;20;0
WireConnection;20;0;15;0
WireConnection;20;1;9;1
WireConnection;7;0;26;0
WireConnection;7;3;12;0
WireConnection;0;0;7;0
ASEEND*/
//CHKSM=6B1E00B0C7AB61227E19BF683E9E7F31064A1AF7