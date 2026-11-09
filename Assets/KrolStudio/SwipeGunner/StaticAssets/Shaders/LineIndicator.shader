Shader "Custom/LineRenderer_Panner"
{
    Properties
    {
        _Texture("Texture", 2D) = "white" {}
        _Albedo("Albedo", Color) = (1,1,1,1)
        _PanSpeed("PanSpeed", Vector) = (-0.5, 0, 0, 0)
        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Texture;
            float4 _Albedo;
            float4 _PanSpeed;
            float _AlphaCutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + _PanSpeed.xy * _Time.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_Texture, i.uv);
                fixed4 col = texCol * _Albedo;

                clip(col.a - _AlphaCutoff);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
