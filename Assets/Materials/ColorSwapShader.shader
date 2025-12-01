Shader "Custom/ReplaceColor" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,0.05)) = 0.01
        _ColorPalette ("Color Palette", 2D) = "white" {}
        _CurrentPalette ("Current Palette", Int) = 0
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest LEqual
        Cull Off
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ColorPalette;
            float _Threshold;
            float _CurrentPalette;
            float _AlphaCutoff;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Discard fully transparent pixels for crisp edges
                if (texColor.a < _AlphaCutoff)
                    discard;

                if (_CurrentPalette == 0)
                    return texColor;

                float paletteWidth = 32;
                float paletteHeight = 32;

                for (float x = 0; x < paletteWidth; x++) {
                    float midU = (x + 0.5) / paletteWidth;
                    float fromV = 1;
                    float toV = 1 - (_CurrentPalette / paletteHeight) - (0.5 / paletteHeight);

                    float4 sourceColor = tex2D(_ColorPalette, float2(midU, fromV));
                    if (distance(texColor.rgb, sourceColor.rgb) < _Threshold) {
                        float4 newColor = tex2D(_ColorPalette, float2(midU, toV));
                        return fixed4(newColor.rgb, texColor.a);
                    }
                }

                return texColor;
            }
            ENDCG
        }
    }
}