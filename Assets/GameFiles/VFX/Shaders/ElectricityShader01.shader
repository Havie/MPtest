// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ElectricityShader01"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_NoiseTexture("NoiseTexture", 2D) = "white" {}
		_DistortionIntensity("DistortionIntensity", Float) = 0.4
		_DistortionPanSpeed("DistortionPanSpeed", Vector) = (0.2,0.1,0,0)
		_DistortionTiling("DistortionTiling", Vector) = (1,1,0,0)
		[Toggle(_USEVERTICALDISTORTION_ON)] _UseVerticalDistortion("UseVerticalDistortion", Float) = 0
		_FXT_Lemon("FXT_Lemon", 2D) = "white" {}
		_LeftBounds("LeftBounds", Float) = -1
		_RightBounds("RightBounds", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
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
				#include "UnityShaderVariables.cginc"
				#pragma shader_feature_local _USEVERTICALDISTORTION_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
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
				uniform float _LeftBounds;
				uniform float _RightBounds;
				uniform sampler2D _NoiseTexture;
				uniform float2 _DistortionPanSpeed;
				uniform float2 _DistortionTiling;
				uniform float _DistortionIntensity;
				uniform sampler2D _FXT_Lemon;
				uniform float4 _FXT_Lemon_ST;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

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

					float2 texCoord4 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord13 = i.texcoord.xy * _DistortionTiling + float2( 0,0 );
					float2 panner11 = ( 1.0 * _Time.y * _DistortionPanSpeed + texCoord13);
					float4 tex2DNode3 = tex2D( _NoiseTexture, panner11 );
					#ifdef _USEVERTICALDISTORTION_ON
					float staticSwitch15 = tex2DNode3.g;
					#else
					float staticSwitch15 = 0.0;
					#endif
					float2 appendResult9 = (float2(tex2DNode3.r , staticSwitch15));
					float lerpResult18 = lerp( _LeftBounds , _RightBounds , appendResult9.x);
					float2 uv_FXT_Lemon = i.texcoord.xy * _FXT_Lemon_ST.xy + _FXT_Lemon_ST.zw;
					float4 tex2DNode1 = tex2D( _MainTex, ( texCoord4 + ( ( lerpResult18 * _DistortionIntensity ) * tex2D( _FXT_Lemon, uv_FXT_Lemon ).r ) ) );
					float4 appendResult25 = (float4(( _TintColor * tex2DNode1 ).rgb , tex2DNode1.a));
					

					fixed4 col = appendResult25;
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
278;491;1172;568;1887.306;556.2095;2.764338;True;False
Node;AmplifyShaderEditor.CommentaryNode;29;-3415.217,36.73883;Inherit;False;814.7109;349.4088;UV Distortion Noise Tiling & Panning. Has significant effect on frequency of distortion.;4;14;12;13;11;UV Distortion Noise Tiling & Panning Controls;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;14;-3365.217,104.9105;Inherit;False;Property;_DistortionTiling;DistortionTiling;3;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;12;-3099.307,222.1476;Inherit;False;Property;_DistortionPanSpeed;DistortionPanSpeed;2;0;Create;True;0;0;0;False;0;False;0.2,0.1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-3105.005,86.73882;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;11;-2805.506,149.8476;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;3;-2534.103,174.9497;Inherit;True;Property;_NoiseTexture;NoiseTexture;0;0;Create;True;0;0;0;False;0;False;-1;431e4e4b162cb584bae8baab7a4b4ebd;9be0d4b3054451844ab59897099e5fcc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;34;-2219.68,-29.79212;Inherit;False;1224.879;547.3872;Distortion Intensity & Direction Tweaking;9;8;19;20;18;9;15;10;6;16;Distortion Intensity & Direction Tweaking;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2169.68,401.5951;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;16;-2211.086,359.774;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;15;-2013.491,316.3753;Inherit;False;Property;_UseVerticalDistortion;UseVerticalDistortion;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;-1725.566,191.6832;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1760.586,96.1027;Inherit;False;Property;_RightBounds;RightBounds;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1760.264,20.20788;Inherit;False;Property;_LeftBounds;LeftBounds;6;0;Create;True;0;0;0;False;0;False;-1;-1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-1554.061,145.4156;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1375.037,202.9812;Inherit;False;Property;_DistortionIntensity;DistortionIntensity;1;0;Create;True;0;0;0;False;0;False;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;33;-968.2881,93.09838;Inherit;False;617.6475;420.4123;Mask Out Distortion to make tips stand still;2;26;27;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;26;-918.2881,283.5107;Inherit;True;Property;_FXT_Lemon;FXT_Lemon;5;0;Create;True;0;0;0;False;0;False;-1;ce718b2391473ba4989998c9a6572793;ce718b2391473ba4989998c9a6572793;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1156.801,146.1858;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;31;-560.9598,-142.0304;Inherit;False;494.914;210.5706;Offset UV's using a texture, warping the texture;2;5;4; UV's using a texture, warping the texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-585.6406,143.0983;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;30;19.30669,-291.503;Inherit;False;1136.861;432.4982;Base;5;22;25;21;1;17;Base;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-510.9598,-90.45981;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;5;-218.0459,-92.03044;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;17;69.30669,-96.91916;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;292.2147,-89.00478;Inherit;True;Property;_MainTexture;MainTexture;0;0;Create;True;0;0;0;False;0;False;-1;585ea85106d359e4099a4c18c928cf45;585ea85106d359e4099a4c18c928cf45;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;21;629.2491,-241.503;Inherit;False;0;0;_TintColor;Shader;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;831.5792,-107.3372;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;985.1672,-106.7658;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StickyNoteNode;36;626.4631,261.0218;Inherit;False;437.5374;163.653;Important Note;Important Note;1,1,1,1;To use this shader you must swap out the shader a material uses to this one.$$To tweak values you should tweak them on the material. Tweaking values here changes the default for all instances of the material.;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1179.798,-103.6921;Float;False;True;-1;2;ASEMaterialInspector;0;7;ElectricityShader01;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;13;0;14;0
WireConnection;11;0;13;0
WireConnection;11;2;12;0
WireConnection;3;1;11;0
WireConnection;16;0;3;2
WireConnection;15;1;10;0
WireConnection;15;0;16;0
WireConnection;9;0;3;1
WireConnection;9;1;15;0
WireConnection;18;0;19;0
WireConnection;18;1;20;0
WireConnection;18;2;9;0
WireConnection;6;0;18;0
WireConnection;6;1;8;0
WireConnection;27;0;6;0
WireConnection;27;1;26;1
WireConnection;5;0;4;0
WireConnection;5;1;27;0
WireConnection;1;0;17;0
WireConnection;1;1;5;0
WireConnection;22;0;21;0
WireConnection;22;1;1;0
WireConnection;25;0;22;0
WireConnection;25;3;1;4
WireConnection;0;0;25;0
ASEEND*/
//CHKSM=A60B4FB4A0EDAA12C6E5024F8C754FA181FA0BA2