// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

Shader "Toony Colors Pro 2/User/My TCP2 Shader"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		_Color ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		_MainTex ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.1
		[TCP2Separator]

		[TCP2HeaderHelp(Emission)]
		[TCP2ColorNoAlpha] [HDR] _Emission ("Emission Color", Color) = (0,0,0,1)
		 [NoScaleOffset] _Emission1 ("Emission Texture", 2D) = "white" {}
		[TCP2Separator]
		
		[TCP2HeaderHelp(Normal Mapping)]
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		[TCP2Separator]
		
		[TCP2HeaderHelp(Outline)]
		_OutlineWidth ("Width", Range(0.1,4)) = 1
		_OutlineColorVertex ("Color", Color) = (0,0,0,1)
		//This property will be ignored and will draw the custom normals GUI instead
		[TCP2OutlineNormalsGUI] __outline_gui_dummy__ ("_unused_", Float) = 0
		[TCP2Separator]

		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
		}

		// Outline Include
		CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc"	// needed for LightColor
		
		// Shader Properties
		float _OutlineWidth;
		fixed4 _OutlineColorVertex;
		sampler2D _BumpMap;
		sampler2D _MainTex;
		float4 _MainTex_ST;
		fixed4 _Color;
		half4 _Emission;
		sampler2D _Emission1;
		float _RampThreshold;
		float _RampSmoothing;
		fixed4 _HColor;
		fixed4 _SColor;

		struct appdata_outline
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		#if TCP2_COLORS_AS_NORMALS
			float4 vertexColor : COLOR;
		#endif
		// TODO: need a way to know if texcoord1 is used in the Shader Properties
		#if TCP2_UV2_AS_NORMALS
			float2 uv2 : TEXCOORD1;
		#endif
			float4 tangent : TANGENT;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f_outline
		{
			float4 vertex : SV_POSITION;
			float4 vcolor : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f_outline vertex_outline (appdata_outline v)
		{
			v2f_outline output;
			UNITY_INITIALIZE_OUTPUT(v2f_outline, output);
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			// Shader Properties Sampling
			float __outlineWidth = ( _OutlineWidth );
			float4 __outlineColorVertex = ( _OutlineColorVertex.rgba );

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		
		#ifdef TCP2_COLORS_AS_NORMALS
			//Vertex Color for Normals
			float3 normal = (v.vertexColor.xyz*2) - 1;
		#elif TCP2_TANGENT_AS_NORMALS
			//Tangent for Normals
			float3 normal = v.tangent.xyz;
		#elif TCP2_UV2_AS_NORMALS
			//UV2 for Normals
			float3 n;
			//unpack uv2
			v.uv2.x = v.uv2.x * 255.0/16.0;
			n.x = floor(v.uv2.x) / 15.0;
			n.y = frac(v.uv2.x) * 16.0 / 15.0;
			//- get z
			n.z = v.uv2.y;
			//- transform
			n = n*2 - 1;
			float3 normal = n;
		#else
			float3 normal = v.normal;
		#endif
			float size = 1;
		
		#if !defined(SHADOWCASTER_PASS)
			output.vertex = UnityObjectToClipPos(v.vertex.xyz + normal * __outlineWidth * size * 0.01);
		#else
			v.vertex = v.vertex + float4(normal,0) * __outlineWidth * size * 0.01;
		#endif
		
			output.vcolor.xyzw = __outlineColorVertex;
			return output;
		}

		float4 fragment_outline (v2f_outline input) : SV_Target
		{
			// Shader Properties Sampling
			float4 __outlineColor = ( float4(1,1,1,1) );

			half4 outlineColor = __outlineColor * input.vcolor.xyzw;
			return outlineColor;
		}

		ENDCG
		// Outline Include End
		// Main Surface Shader

		CGPROGRAM

		#pragma surface surf ToonyColorsCustom vertex:vertex_surface exclude_path:deferred exclude_path:prepass keepalpha nolightmap nofog nolppv
		#pragma target 3.0

		//================================================================
		// VARIABLES

		//Vertex input
		struct appdata_tcp2
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord0 : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			half4 tangent : TANGENT;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			half3 tangent;
			float2 texcoord0;
		};

		//================================================================
		// VERTEX FUNCTION

		void vertex_surface(inout appdata_tcp2 v, out Input output)
		{
			UNITY_INITIALIZE_OUTPUT(Input, output);

			// Texture Coordinates
			output.texcoord0.xy = v.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			output.tangent = mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0)).xyz;
		}

		//================================================================

		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			half atten;
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Specular;
			half Gloss;
			half Alpha;

			Input input;
			
			// Shader Properties
			float __rampThreshold;
			float __rampSmoothing;
			float3 __highlightColor;
			float3 __shadowColor;
			float __ambientIntensity;
		};

		//================================================================
		// SURFACE FUNCTION

		void surf(Input input, inout SurfaceOutputCustom output)
		{
			// Shader Properties Sampling
			float4 __normalMap = ( tex2D(_BumpMap, input.texcoord0.xy).rgba );
			float4 __albedo = ( tex2D(_MainTex, input.texcoord0.xy).rgba );
			float4 __mainColor = ( _Color.rgba );
			float __alpha = ( __albedo.a * __mainColor.a );
			float3 __emission = ( _Emission.rgb * tex2D(_Emission1, input.texcoord0.xy).rgb );
			output.__rampThreshold = ( _RampThreshold );
			output.__rampSmoothing = ( _RampSmoothing );
			output.__highlightColor = ( _HColor.rgb );
			output.__shadowColor = ( _SColor.rgb );
			output.__ambientIntensity = ( 1.0 );

			output.input = input;

			// Normal Mapping
			half4 normalMap = __normalMap;
			output.Normal = UnpackNormal(normalMap);

			output.Albedo = __albedo.rgb;
			output.Alpha = __alpha;
			
			output.Albedo *= __mainColor.rgb;
			output.Emission += __emission;
		}

		//================================================================
		// LIGHTING FUNCTION

		inline half4 LightingToonyColorsCustom(inout SurfaceOutputCustom surface, UnityGI gi)
		{
			half3 lightDir = gi.light.dir;
			#if defined(UNITY_PASS_FORWARDBASE)
				half3 lightColor = _LightColor0.rgb;
				half atten = surface.atten;
			#else
				//extract attenuation from point/spot lights
				half3 lightColor = _LightColor0.rgb;
				half atten = max(gi.light.color.r, max(gi.light.color.g, gi.light.color.b)) / max(_LightColor0.r, max(_LightColor0.g, _LightColor0.b));
			#endif

			half3 normal = normalize(surface.Normal);
			half ndl = dot(normal, lightDir);
			half3 ramp;
			
			#define		RAMP_THRESHOLD	surface.__rampThreshold
			#define		RAMP_SMOOTH		surface.__rampSmoothing
			ndl = saturate(ndl);
			ramp = smoothstep(RAMP_THRESHOLD - RAMP_SMOOTH*0.5, RAMP_THRESHOLD + RAMP_SMOOTH*0.5, ndl);
			half3 rampGrayscale = ramp;

			//Apply attenuation (shadowmaps & point/spot lights attenuation)
			ramp *= atten;

			//Highlight/Shadow Colors
			#if !defined(UNITY_PASS_FORWARDBASE)
				ramp = lerp(half3(0,0,0), surface.__highlightColor, ramp);
			#else
				ramp = lerp(surface.__shadowColor, surface.__highlightColor, ramp);
			#endif

			//Output color
			half4 color;
			color.rgb = surface.Albedo * lightColor.rgb * ramp;
			color.a = surface.Alpha;

			// Apply indirect lighting (ambient)
			half occlusion = 1;
			#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
				half3 ambient = gi.indirect.diffuse;
				ambient *= surface.Albedo * occlusion * surface.__ambientIntensity;

				color.rgb += ambient;
			#endif

			return color;
		}

		void LightingToonyColorsCustom_GI(inout SurfaceOutputCustom surface, UnityGIInput data, inout UnityGI gi)
		{
			half3 normal = surface.Normal;

			//GI without reflection probes
			gi = UnityGlobalIllumination(data, 1.0, normal); // occlusion is applied in the lighting function, if necessary

			surface.atten = data.atten; // transfer attenuation to lighting function
			gi.light.color = _LightColor0.rgb; // remove attenuation
		}

		ENDCG

		//Outline
		Pass
		{
			Name "Outline"
			Tags { "LightMode"="ForwardBase" }
			Cull Front
			Blend Off

			CGPROGRAM
			#pragma vertex vertex_outline
			#pragma fragment fragment_outline
			#pragma target 3.0
			#pragma multi_compile TCP2_NONE TCP2_COLORS_AS_NORMALS TCP2_TANGENT_AS_NORMALS TCP2_UV2_AS_NORMALS
			#pragma multi_compile_instancing
			ENDCG
		}
	}

	Fallback "Diffuse"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(unity:"2020.3.25f1";ver:"2.4.5";tmplt:"SG2_Template_Default";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","EMISSION","BUMP","OUTLINE","OUTLINE_OPAQUE"];flags:list[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0"];shaderProperties:list[,,,,,,,,,,,,,,,,,,,,sp(name:"Emission";imps:list[imp_mp_color(def:RGBA(0.000, 0.000, 0.000, 1.000);hdr:True;cc:3;chan:"RGB";prop:"_Emission";md:"";custom:False;refs:"";guid:"a6712bd3-1831-4d93-b5ff-e76e79fa0025";op:Multiply;lbl:"Emission Color";gpu_inst:False;locked:False;impl_index:0),imp_mp_texture(uto:False;tov:"";gto:False;sbt:False;scr:False;scv:"";gsc:False;roff:False;goff:False;notile:False;def:"white";locked_uv:False;uv:0;cc:3;chan:"RGB";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";uv_shaderproperty:__NULL__;prop:"_Emission1";md:"";custom:False;refs:"";guid:"d2c1c063-b848-45cf-93fa-8b3ded364726";op:Multiply;lbl:"Emission Texture";gpu_inst:False;locked:False;impl_index:-1)])];customTextures:list[]) */
/* TCP_HASH a1cc67e8a5ba33ba338b8e42fe47a4e9 */
