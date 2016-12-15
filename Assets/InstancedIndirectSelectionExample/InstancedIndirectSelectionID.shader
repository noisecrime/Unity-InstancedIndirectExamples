Shader "Instanced/InstancedIndirectSelectionID" 
{
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag		
			#pragma target 4.5

			#include "UnityCG.cginc"
			
	#if SHADER_TARGET >= 45
			StructuredBuffer<float4> positionBuffer;
	#endif

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color: COLOR;
			};

			void rotate2D(inout float2 v, float r)
			{
				float s, c;
				sincos(r, s, c);
				v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
			}

			// Encoding uint to RGBA but not using alpha so we can display renderTexture as overlay if desired.
			// This means max ID is limited to 16,777,214 as 16,777,215 is deemed to be 'empty' or no selection.
			// ID starts at zero.
			float4 EncodeIntToRGBA( uint id)
			{
				return float4(((id & 0x000000FF) >> 0)/255.0f, ((id & 0x0000FF00) >> 8)/255.0f, ((id & 0x00FF0000) >> 16)/255.0f, 1.0f); // (id & 0xFF000000) >> 24);
			}


			v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
			{
		#if SHADER_TARGET >= 45
				float4 data = positionBuffer[instanceID];				
		#else
				float4 data = 0;
		#endif

				float rotation = data.w * data.w * _Time.y * 0.5f;
				rotate2D(data.xz, rotation);

				float3 localPosition = v.vertex.xyz * data.w;
				float3 worldPosition = data.xyz + localPosition;

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));	

#if SHADER_TARGET >= 45
				o.color = EncodeIntToRGBA(instanceID); //  float4((float)(instanceID % 256) / 255.0f, 0, 0, 1);
#else
				o.color = float4(0, 0, 1, 1);
#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}

			ENDCG
		}
	}
}
