// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "StandardWithOutline"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_DetailMask("DetailMask", 2D) = "white" {}
		_MetallicMultiply("Metallic Multiply", Float) = 1
		_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		_RoughnessMultiply("Roughness Multiply", Float) = 1
		_EmissiveValue("Emissive Value", Float) = 1
		[Toggle(_EMISSIVE_ON)] _Emissive("Emissive", Float) = 0
		_OutlineColor("OutlineColor", Color) = (0,0,0,1)
		_OutlineWidth("OutlineWidth", Float) = 0.1
		_Color("Color", Color) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0"}
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog alpha:fade  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _OutlineWidth;
			v.vertex.xyz *= ( 1 + outlineVar);
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _OutlineColor.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
		

		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Add
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature_local _EMISSIVE_ON
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform sampler2D _DetailMask;
		uniform float4 _DetailMask_ST;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveValue;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicMultiply;
		uniform float _RoughnessMultiply;
		uniform float4 _OutlineColor;
		uniform float _OutlineWidth;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode14 = tex2D( _Albedo, uv_Albedo );
			float3 appendResult44 = (float3(_Color.r , _Color.g , _Color.b));
			o.Albedo = ( tex2DNode14 * float4( appendResult44 , 0.0 ) ).rgb;
			float4 temp_cast_2 = (0.0).xxxx;
			float2 uv_DetailMask = i.uv_texcoord * _DetailMask_ST.xy + _DetailMask_ST.zw;
			#ifdef _EMISSIVE_ON
				float4 staticSwitch35 = ( ( tex2D( _DetailMask, uv_DetailMask ) + _EmissiveColor ) * _EmissiveValue );
			#else
				float4 staticSwitch35 = temp_cast_2;
			#endif
			o.Emission = staticSwitch35.rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			float4 tex2DNode19 = tex2D( _Metallic, uv_Metallic );
			o.Metallic = ( tex2DNode19 * _MetallicMultiply ).r;
			o.Smoothness = ( tex2DNode19.a * _RoughnessMultiply );
			o.Alpha = ( _Color.a * tex2DNode14.a );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
1111;73;808;966;1959.666;2837.728;2.817377;True;False
Node;AmplifyShaderEditor.CommentaryNode;62;-888.3665,-1116.173;Inherit;False;1315.474;1294.505;Standard Lit Parameters. ;19;32;27;43;34;16;19;30;29;44;14;2;26;36;17;42;45;35;7;71;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-758.6734,415.1888;Inherit;False;1712.322;1409.261;Outline / Highlight;6;56;40;55;58;61;70;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;32;-550.5456,-302.8423;Inherit;False;Property;_EmissiveColor;Emissive Color;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;27;-838.3665,-380.1629;Inherit;True;Property;_DetailMask;DetailMask;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;56;-708.6734,1109.546;Inherit;False;1008.687;714.9047;This outline scales in size depending on camera distance, but looks bad if width is too large.;6;47;48;46;41;52;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;43;-586.0056,-1066.173;Inherit;False;Property;_Color;Color;12;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-311.7807,-376.3372;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-247.8391,-233.3689;Inherit;False;Property;_EmissiveValue;Emissive Value;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-305.0251,1159.546;Inherit;False;Property;_OutlineWidth;OutlineWidth;11;0;Create;True;0;0;0;False;0;False;0.1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-384.8593,-42.66781;Inherit;False;Property;_MetallicMultiply;Metallic Multiply;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;19;-701.7455,-53.04371;Inherit;True;Property;_Metallic;Metallic;1;0;Create;True;0;0;0;False;0;False;-1;85001e1a64aec8a48a92409cd861b597;85001e1a64aec8a48a92409cd861b597;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;44;-340.1593,-1039.287;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;40;-328.44,770.8889;Inherit;False;Property;_OutlineColor;OutlineColor;10;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-5.20129,-368.5804;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;14;-676.1929,-879.2922;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;db83870432c993a4a844832e0278b5cc;db83870432c993a4a844832e0278b5cc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-400.8593,62.33212;Inherit;False;Property;_RoughnessMultiply;Roughness Multiply;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;55;407.9293,465.1888;Inherit;False;495.7196;238.2709;Use this instead to get less jaggy outlines, but might not line up 100%.;1;38;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-10.22319,-447.0416;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;58;390.7414,727.403;Inherit;False;511.7326;399.8456;Use this for completely transparent outlines;3;67;57;69;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-146.5642,40.12345;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;203.4693,-563.0475;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;61;205.8569,771.3309;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OutlineNode;38;452.051,535.6813;Inherit;False;1;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StickyNoteNode;73;1278.484,441.5019;Inherit;False;313.1045;346.399;For programmers;;1,1,1,1;To make the geometry more transparent you tweak the _Color material property in the alpha channel (0-1). $$(Note: This only works if the blendmode is set to transparent)$$$$To tweak the color and transparency of the outline you tweak the _OutlineColor material property. $$(Note: This only works if the outline nodes using the transparent method are plugged in.)$$;0;0
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;46;-449.4926,1617.45;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-122.1248,1308.527;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;47;-658.6734,1617.451;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-134.9451,-61.04365;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-69.45338,-869.4263;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;35;204.1077,-394.3349;Inherit;False;Property;_Emissive;Emissive;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OutlineNode;52;47.01377,1159.651;Inherit;False;2;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OutlineNode;67;457.2434,949.0704;Inherit;False;1;True;Transparent;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;26;-668.7062,-635.0732;Inherit;True;Property;_Normal;Normal;3;0;Create;True;0;0;0;False;0;False;-1;d0483f22007cf2f419e2707453b3e367;d0483f22007cf2f419e2707453b3e367;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OutlineNode;57;464.4996,779.3827;Inherit;False;2;True;Transparent;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StickyNoteNode;69;682.7177,819.942;Inherit;False;178.0542;188.1703;Cost;;1,1,1,1;These are more expensive but without them you get a solid color (the outlines) when you fade out the transparent geometry. Not as helpful if geometry is opaque.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;70;431.18,1166.439;Inherit;False;351.8929;161.288;Note;;1,1,1,1;If node had alpha input then it is transparent. $$If it says Opacity Mask then it is "masked" which is essentially opaque except you can make it visible/not visible more cheaply than transparent.;0;0
Node;AmplifyShaderEditor.StickyNoteNode;71;141.2727,-1034.76;Inherit;False;241.1173;196.931;What are these?;;1,1,1,1;These nodes recreate the base function / inputs used in Unity's standard lit shader. Consider this entire shader an "extension" of the default Unity standard shader.$;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;48;-455.894,1338.845;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StickyNoteNode;72;1274.552,223.2922;Inherit;False;292.3044;186.399;Performance Notes:;;1,1,1,1;Most performant solution would be opaque geometry  with masked outline.$$Transparent geometry should probably use the scaled (non-custom) transparent outline.$$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;66;1231.2,1.528697;Inherit;False;356;177;How to: Make geometry opaque/transparent.;;1,1,1,1;Click an empty area, then go to the output node menu on the left and change the blend mode to transparent.$$The outline blend mode is changed in each individual "outline" node.;0;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1305.758,-443.599;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;StandardWithOutline;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;True;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;19.67;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;27;0
WireConnection;16;1;32;0
WireConnection;44;0;43;1
WireConnection;44;1;43;2
WireConnection;44;2;43;3
WireConnection;17;0;16;0
WireConnection;17;1;34;0
WireConnection;7;0;19;4
WireConnection;7;1;30;0
WireConnection;45;0;43;4
WireConnection;45;1;14;4
WireConnection;61;0;40;0
WireConnection;61;3;40;3
WireConnection;38;0;40;0
WireConnection;38;2;40;4
WireConnection;38;1;41;0
WireConnection;46;0;47;0
WireConnection;50;0;48;0
WireConnection;50;1;41;0
WireConnection;50;2;46;4
WireConnection;2;0;19;0
WireConnection;2;1;29;0
WireConnection;42;0;14;0
WireConnection;42;1;44;0
WireConnection;35;1;36;0
WireConnection;35;0;17;0
WireConnection;52;0;40;0
WireConnection;52;2;40;4
WireConnection;52;1;50;0
WireConnection;67;0;40;0
WireConnection;67;2;43;4
WireConnection;67;1;41;0
WireConnection;57;0;61;0
WireConnection;57;2;43;4
WireConnection;57;1;50;0
WireConnection;0;0;42;0
WireConnection;0;1;26;0
WireConnection;0;2;35;0
WireConnection;0;3;2;0
WireConnection;0;4;7;0
WireConnection;0;9;45;0
WireConnection;0;11;67;0
ASEEND*/
//CHKSM=1ED5825171AE97F5C78B1A878A988D3CE4B46A28