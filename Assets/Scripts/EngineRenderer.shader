Shader "Unlit/Engine"
{
    Properties
    {
        _COLOR_1 ("Color 1", Color) = (1.0, 0.0, 1.0, 1.0) // red
        _COLOR_2 ("Color 2", Color) = (1.0, 1.0, 0.0, 1.0) // yellow
        _COLOR_3 ("Color 3", Color) = (0.0, 1.0, 1.0, 1.0) // blue
        _COLOR_4 ("Color 4", Color) = (1.0, 0.5, 1.0, 1.0) // orange
        _COLOR_5 ("Color 5", Color) = (1.0, 0.0, 1.0, 1.0) // purple
        _COLOR_6 ("Color 6", Color) = (0.0, 0.8, 1.0, 1.0) // green
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            //#include "UnityCG.cginc"
            //#include "C:\Program Files\Unity\Hub\Editor\2022.3.7f1\Editor\Data\CGIncludes\UnityCG.cginc"

            struct Particle
            {
                float2 position;
                float2 velocity;
                float2 force;  // integer to allow for InterlockedAdd
                uint c;
                int domain;
                int active;
                // also add # neighbors - engine
            };

            StructuredBuffer<Particle> particles;

            float4 _COLOR_1;
            float4 _COLOR_2;
            float4 _COLOR_3;
            float4 _COLOR_4;
            float4 _COLOR_5;
            float4 _COLOR_6;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                uint c : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v, const uint instance_id : SV_InstanceID)
            {
                Particle p = particles[instance_id];

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + float4(p.position, 0, 1));
                o.c = p.c;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target // add another case to draw it in highlighted color
            {
                switch (i.c + 1)
                {
                    case 0:
                        return float4(1, 1, 1, 1);
                    case 1:
                        return _COLOR_1;
                    case 2:
                        return _COLOR_2;
                    case 3:
                        return _COLOR_3;
                    case 4:
                        return _COLOR_4;
                    case 5:
                        return _COLOR_5;
                    case 6:
                        return _COLOR_6;
                }
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
