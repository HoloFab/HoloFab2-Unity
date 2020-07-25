Shader "HoloFab/VertexColor" {
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert vertex:vert alpha
#pragma target 3.0

		struct Input {
		float4 vertColor;
	};

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.vertColor = v.color;
	}

	void surf(Input IN, inout SurfaceOutput o) {
		o.Albedo = IN.vertColor.rgb;
		o.Alpha = .5;
	}
	ENDCG
	}
		FallBack "Diffuse"
}