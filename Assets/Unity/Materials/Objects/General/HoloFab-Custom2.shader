Shader "HoloFab/Custom2"
{
	// Vertex based color shader for meshes, receiving and casting shadows.
	Properties {
		//_TintColor ("Main Tint Color", Color) = (1, 1, 1, 1)
		_Alpha("Alpha", Range(0.0, 1.0)) = 0.5
		_ShadowStrength("Shadow Strength", Range(0.0, 1.0)) = 1
		//_MainTexture ("Main Texture", 2D) = "white" {}
	}
	SubShader {
		//////////////////////////////////////////////////////////////////////////
		// Special pass for depth testing.
		Pass {
			// Render depth
			ZWrite On
			// Don't render color
			ColorMask 0
		}
		// Main Pass.
		Pass {
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "LightMode"="ForwardBase" }
			// Turn off depth testing - necessary for transparency
			ZWrite Off
			// Turn off back surface culling
			Cull Off
			ColorMask RGBA
			// Turn on Blending for alpha
			Blend SrcAlpha OneMinusSrcAlpha

			// Actual Shader
			CGPROGRAM
			// Shader function declarations
			#pragma vertex vert
			//#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			// Includes
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			//#include "UnityInstancing.gcinc"

			///////////////////////////////////////////////////////////////////////
			// vertex shader inputs
			struct vertexShaderData {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 position : POSITION; // vertex position
				float3 normal : NORMAL; // vertex normal
				//float2 uv : TEXCOORD0; // texture coordinate
				fixed3 vertexColor : COLOR0; // vertex color
			};

			// fragment shader inputs
			struct fragmentShaderData {
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 position : SV_POSITION; // Screen vertex position
				float3 normal : NORMAL; // vertex normal
				//float3 worldPosition : TEXCOORD0; // World position
				//float2 uv : TEXCOORD1; // texture coordinate
				//SHADOW_COORDS(2) // put shadows data into TEXCOORD1
				fixed4 vertexColor : COLOR0; // vertex color
				fixed4 diffuseColor : COLOR1; // diffuse lighting color
				fixed3 ambientColor : COLOR2; // ambient lighting color
			};

			// fragment shader outputs
			struct fragmentShaderOutput {
				fixed4 color : SV_Target;
			};
			///////////////////////////////////////////////////////////////////////
			// Vertex Shader.
			// - shadow strength
			float _ShadowStrength;
			fragmentShaderData vert(vertexShaderData IN) {
				fragmentShaderData output;
				//UNITY_INSTANTIAATE_OUTPUT(fragmentShaderData output);
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.position = UnityObjectToClipPos(IN.position);
				output.normal = IN.normal;
				//output.worldPosition = mul(UNITY_MATRIX_MVP, IN.position);
				//output.uv = IN.uv;
				half3 worldNormal = UnityObjectToWorldNormal(IN.normal);

				// Color
				output.vertexColor.rgb = IN.vertexColor;// ShadeVertexLights(IN.position, IN.normal);
				output.vertexColor.a = _ShadowStrength;

				// Diffuse Lighting
				half normalMagnitude = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				output.diffuseColor = normalMagnitude * _LightColor0;

				// Ambient lighting
				output.ambientColor = ShadeSH9(half4(worldNormal, 1));

				// Compute shadow data
				//TRANSFER_SHADOW(output)

				return output;
			}

			///////////////////////////////////////////////////////////////////////
			// // Geometry Shader
			// // Duplicate faces in reverse order
			//[maxvertexcount(6)]
			//void geom (triangle fragmentShaderData IN[3], inout TriangleStream<fragmentShaderData> output) {
			//	for (int i = 0; i < 3; i++) {
			//		fragmentShaderData currentPoint = IN[i];
			//		currentPoint.vertexColor.a = 0;
			//		output.Append(currentPoint);
			//	}
			//	output.RestartStrip();
			//	for (int j = 0; j < 3; j++) {
			//	//for (int j = 2; j >= 0; j--) {
			//		fragmentShaderData currentPoint = IN[j];
			//		//currentPoint.vertexColor.a = 0;
			//		output.Append(currentPoint);
			//	}
			//	output.RestartStrip();
			//}

			///////////////////////////////////////////////////////////////////////
			// Fragment Shader.
			// Inputs from outside the shader:
			// - alpha
			float _Alpha;
			//// - tint color
			//float4 _TintColor;
			//// - texture
			//sampler2D _MainTexture;
			fragmentShaderOutput frag (fragmentShaderData IN) {
				fragmentShaderOutput output;
				UNITY_SETUP_INSTANCE_ID(IN);

				// Apply Colors
				output.color = IN.vertexColor;
				output.color.a = _Alpha;
				//output.color *= _TintColor;

				// Apply Shadows
				fixed shadowAttenuation = 1;
				//fixed shadowAttenuation = SHADOW_ATTENUATION(IN);
				fixed3 lighting = IN.diffuseColor * shadowAttenuation + IN.ambientColor;
				output.color.rgb *= (1 - (1-lighting) * IN.vertexColor.a);

				// Apply Texture
				//fixed4 texture = tex2D (_MainTexture, IN.uv);
				//output.color.rgb *= texture.rgb;
				//output.color.a *= texture.a;

				return output;
			}
			ENDCG
		}
		//////////////////////////////////////////////////////////////////////////
		// A Pass to cast shadows.
		Pass {
			Tags {"LightMode"="ShadowCaster"}

			// Actual Shader
			CGPROGRAM
			// Shader function declarations
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			// Includes
			#include "UnityCG.cginc"

			// fragment shader inputs
			struct v2f {
			    V2F_SHADOW_CASTER;
			};

			///////////////////////////////////////////////////////////////////////
			// Vertex Shader.
			v2f vert(appdata_base v) {
				v2f o;

			    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

			    return o;
			}

			///////////////////////////////////////////////////////////////////////
			// Fragment Shader.
			float4 frag(v2f IN) : SV_Target {
			    SHADOW_CASTER_FRAGMENT(IN)
			}
			ENDCG
		}
	}
}
