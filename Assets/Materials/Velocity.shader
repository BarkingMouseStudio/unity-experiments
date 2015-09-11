Shader "Custom/Velocity" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
      #include "UnityCG.cginc"

			float4 vert(float4 v:POSITION) : SV_POSITION {
				return mul (UNITY_MATRIX_MVP, v);
			}

			fixed4 frag() : SV_Target {
				return fixed4(1.0,0.0,0.0,1.0);
			}

			ENDCG
		}
	}
}
