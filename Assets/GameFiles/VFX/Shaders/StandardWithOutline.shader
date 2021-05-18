// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "StandardWithOutline"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.05
		_Normal("Normal", 2D) = "bump" {}
		_MetallicMultiply("Metallic Multiply", Float) = 1
		_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		_RoughnessMultiply("Roughness Multiply", Float) = 1
		_EmissiveValue("Emissive Value", Float) = 1
		[Toggle(_EMISSIVE_ON)] _Emissive("Emissive", Float) = 0
		_OutlineColor("OutlineColor", Color) = (0,0,0,1)
		_OutlineWidth("OutlineWidth", Float) = 0.1
		_Color("Color", Color) = (1,1,1,1)
		_MaskedOpacityDitherIntensity("MaskedOpacityDitherIntensity", Float) = 0.2
		[Toggle(_GEOMETRYFADEWITHOUTLINE_ON)] _GeometryFadeWithOutline("GeometryFadeWithOutline", Float) = 0
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
			#ifdef _GEOMETRYFADEWITHOUTLINE_ON
				float staticSwitch82 = ( _Color.b * _OutlineColor.a );
			#else
				float staticSwitch82 = _OutlineColor.a;
			#endif
			o.Emission = _OutlineColor.rgb;
			o.Alpha = staticSwitch82;
		}
		ENDCG
		

		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Add
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature_local _EMISSIVE_ON
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPosition;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform float4 _EmissiveColor;
		uniform float _EmissiveValue;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicMultiply;
		uniform float _RoughnessMultiply;
		uniform float _MaskedOpacityDitherIntensity;
		uniform float _Cutoff = 0.05;
		uniform float4 _OutlineColor;
		uniform float _OutlineWidth;


		inline float Dither8x8Bayer( int x, int y )
		{
			const float dither[ 64 ] = {
				 1, 49, 13, 61,  4, 52, 16, 64,
				33, 17, 45, 29, 36, 20, 48, 32,
				 9, 57,  5, 53, 12, 60,  8, 56,
				41, 25, 37, 21, 44, 28, 40, 24,
				 3, 51, 15, 63,  2, 50, 14, 62,
				35, 19, 47, 31, 34, 18, 46, 30,
				11, 59,  7, 55, 10, 58,  6, 54,
				43, 27, 39, 23, 42, 26, 38, 22};
			int r = y * 8 + x;
			return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float3 appendResult44 = (float3(_Color.r , _Color.g , _Color.b));
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * float4( appendResult44 , 0.0 ) ).rgb;
			float4 temp_cast_2 = (0.0).xxxx;
			#ifdef _EMISSIVE_ON
				float4 staticSwitch35 = ( _EmissiveColor * _EmissiveValue );
			#else
				float4 staticSwitch35 = temp_cast_2;
			#endif
			o.Emission = staticSwitch35.rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			float4 tex2DNode19 = tex2D( _Metallic, uv_Metallic );
			o.Metallic = ( tex2DNode19 * _MetallicMultiply ).r;
			o.Smoothness = ( tex2DNode19.a * _RoughnessMultiply );
			o.Alpha = _Color.a;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen77 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither77 = Dither8x8Bayer( fmod(clipScreen77.x, 8), fmod(clipScreen77.y, 8) );
			dither77 = step( dither77, _MaskedOpacityDitherIntensity );
			clip( dither77 - _Cutoff );
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
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 tSpace0 : TEXCOORD4;
				float4 tSpace1 : TEXCOORD5;
				float4 tSpace2 : TEXCOORD6;
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
				o.customPack2.xyzw = customInputData.screenPosition;
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
				surfIN.screenPosition = IN.customPack2.xyzw;
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
0;0;1824;964;2476.81;1571.299;2.687823;False;False
Node;AmplifyShaderEditor.CommentaryNode;62;-888.3665,-1116.173;Inherit;False;1315.474;1294.505;Standard Lit Parameters. ;19;32;27;43;34;16;19;30;29;44;14;2;26;36;17;42;35;7;71;76;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-785.0101,437.7632;Inherit;False;1712.322;1409.261;Outline / Highlight;8;56;40;55;58;61;70;83;82;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;43;-586.0056,-1066.173;Inherit;False;Property;_Color;Color;12;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;40;-354.7768,793.4633;Inherit;False;Property;_OutlineColor;OutlineColor;10;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;56;-735.0101,1132.12;Inherit;False;1008.687;714.9047;This outline scales in size depending on camera distance, but looks bad if width is too large.;6;47;48;46;41;52;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;32;-550.5456,-302.8423;Inherit;False;Property;_EmissiveColor;Emissive Color;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-323.4932,-250.1809;Inherit;False;Property;_EmissiveValue;Emissive Value;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-159.3057,498.0612;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-676.1929,-879.2922;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;db83870432c993a4a844832e0278b5cc;db83870432c993a4a844832e0278b5cc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;55;381.5925,487.7632;Inherit;False;495.7196;238.2709;Use this instead to get less jaggy outlines, but might not line up 100%.;1;38;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;58;364.4046,749.9774;Inherit;False;511.7326;399.8456;Use this for completely transparent outlines;3;67;57;69;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;44;-340.1593,-1039.287;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;19;-701.7455,-53.04371;Inherit;True;Property;_Metallic;Metallic;1;0;Create;True;0;0;0;False;0;False;-1;85001e1a64aec8a48a92409cd861b597;85001e1a64aec8a48a92409cd861b597;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-400.8593,62.33212;Inherit;False;Property;_RoughnessMultiply;Roughness Multiply;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;82;-4.044233,496.6455;Inherit;False;Property;_GeometryFadeWithOutline;GeometryFadeWithOutline;14;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-384.8593,-42.66781;Inherit;False;Property;_MetallicMultiply;Metallic Multiply;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-331.3619,1182.12;Inherit;False;Property;_OutlineWidth;OutlineWidth;11;0;Create;True;0;0;0;False;0;False;0.1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-14.54172,-296.7983;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-10.22319,-447.0416;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;721.3063,-98.31715;Inherit;False;Property;_MaskedOpacityDitherIntensity;MaskedOpacityDitherIntensity;13;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-148.4616,1331.101;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-138.0162,-375.3052;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-134.9451,-61.04365;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DitheringNode;77;1009.44,-98.93774;Inherit;False;1;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StickyNoteNode;73;1278.484,441.5019;Inherit;False;313.1045;346.399;For programmers;;1,1,1,1;To make the geometry more transparent you tweak the _Color material property in the alpha channel (0-1). $$(Note: This only works if the blendmode is set to transparent)$$$$To tweak the color and transparency of the outline you tweak the _OutlineColor material property. $$(Note: This only works if the outline nodes using the transparent method are plugged in.)$$;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-69.45338,-869.4263;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-311.7807,-376.3372;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;27;-838.3665,-380.1629;Inherit;True;Property;_DetailMask;DetailMask;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-146.5642,40.12345;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;47;-685.0101,1640.026;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;26;-668.7062,-635.0732;Inherit;True;Property;_Normal;Normal;3;0;Create;True;0;0;0;False;0;False;-1;d0483f22007cf2f419e2707453b3e367;d0483f22007cf2f419e2707453b3e367;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;35;204.1077,-394.3349;Inherit;False;Property;_Emissive;Emissive;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StickyNoteNode;66;1231.2,1.528697;Inherit;False;356;177;How to: Make geometry opaque/transparent.;;1,1,1,1;Click an empty area, then go to the output node menu on the left and change the blend mode to transparent.$$The outline blend mode is changed in each individual "outline" node.;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;48;-482.2308,1361.419;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StickyNoteNode;71;141.2727,-1034.76;Inherit;False;241.1173;196.931;What are these?;;1,1,1,1;These nodes recreate the base function / inputs used in Unity's standard lit shader. Consider this entire shader an "extension" of the default Unity standard shader.$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;69;656.381,842.5164;Inherit;False;178.0542;188.1703;Cost;;1,1,1,1;These are more expensive but without them you get a solid color (the outlines) when you fade out the transparent geometry. Not as helpful if geometry is opaque.;0;0
Node;AmplifyShaderEditor.OutlineNode;52;20.67698,1182.225;Inherit;False;2;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OutlineNode;57;438.1628,801.9571;Inherit;False;2;True;Transparent;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;46;-475.8294,1640.024;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StickyNoteNode;72;1274.552,223.2922;Inherit;False;292.3044;186.399;Performance Notes:;;1,1,1,1;Most performant solution would be opaque geometry  with masked outline.$$Transparent geometry should probably use the scaled (non-custom) transparent outline.$$;0;0
Node;AmplifyShaderEditor.StickyNoteNode;70;421.4509,1460.775;Inherit;False;351.8929;161.288;Note;;1,1,1,1;If node had alpha input then it is transparent. $$If it says Opacity Mask then it is "masked" which is essentially opaque except you can make it visible/not visible more cheaply than transparent.;0;0
Node;AmplifyShaderEditor.OutlineNode;38;425.7142,558.2557;Inherit;False;1;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StickyNoteNode;84;1612.457,4.025948;Inherit;False;983.1051;197.4446;Material Properties to Focusu on;;1,1,1,1;_Color (Alpha Channel controls geometry opacity, does not work with opaque mode)$$_OutlineColor (Alpha Channel controls outline opacity, behaves different if using a masked/transparent outline but generally toggles its visibility if you use 0-1)$$_EmissiveValue (Only works if the emissive Toggle is ON)$$_MaskedOpacityDitherIntensity (If you wish to use the transparent cutout dither method this takes an opacity vaue of 0-1);0;0
Node;AmplifyShaderEditor.DynamicAppendNode;61;179.5201,793.9053;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OutlineNode;67;430.9066,971.6449;Inherit;False;1;True;Transparent;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1325.758,-443.599;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;StandardWithOutline;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;True;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.05;True;True;0;True;TransparentCutout;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;19.67;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;83;0;43;3
WireConnection;83;1;40;4
WireConnection;44;0;43;1
WireConnection;44;1;43;2
WireConnection;44;2;43;3
WireConnection;82;1;40;4
WireConnection;82;0;83;0
WireConnection;76;0;32;0
WireConnection;76;1;34;0
WireConnection;50;0;48;0
WireConnection;50;1;41;0
WireConnection;50;2;46;4
WireConnection;17;0;16;0
WireConnection;17;1;34;0
WireConnection;2;0;19;0
WireConnection;2;1;29;0
WireConnection;77;0;78;0
WireConnection;42;0;14;0
WireConnection;42;1;44;0
WireConnection;16;0;27;0
WireConnection;16;1;32;0
WireConnection;7;0;19;4
WireConnection;7;1;30;0
WireConnection;35;1;36;0
WireConnection;35;0;76;0
WireConnection;52;0;40;0
WireConnection;52;2;82;0
WireConnection;52;1;50;0
WireConnection;57;0;61;0
WireConnection;57;2;82;0
WireConnection;57;1;50;0
WireConnection;46;0;47;0
WireConnection;38;0;40;0
WireConnection;38;2;82;0
WireConnection;38;1;41;0
WireConnection;61;0;40;0
WireConnection;61;3;40;3
WireConnection;67;0;40;0
WireConnection;67;2;82;0
WireConnection;67;1;41;0
WireConnection;0;0;42;0
WireConnection;0;1;26;0
WireConnection;0;2;35;0
WireConnection;0;3;2;0
WireConnection;0;4;7;0
WireConnection;0;9;43;4
WireConnection;0;10;77;0
WireConnection;0;11;67;0
ASEEND*/
//CHKSM=A4E248A3DF21859EEA20D7F98C46F99C3F457BE6