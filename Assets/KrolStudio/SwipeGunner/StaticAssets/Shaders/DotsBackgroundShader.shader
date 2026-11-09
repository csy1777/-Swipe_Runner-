Shader "Custom/DotsBackgroundShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _DotsColor ("Dots Color", Color) = (0,1,1,1)
        _AlphaControl ("Alpha Control", Float) = 1
        _Size ("Size", Float) = 10
        _RotationAngle ("Rotation Angle", Float) = 0
        _DotsAngle ("Dots Angle", Float) = 0
        _MovingSpeed ("Moving Speed", Vector) = (0, 1, 0, 0)
        _MinDotSize ("Min Dot Size", Float) = 0.1
        _MaxDotSize ("Max Dot Size", Float) = 0.4
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BaseColor;
            float4 _DotsColor;
            float _Size;
            float _RotationAngle;
            float _DotsAngle;
            float2 _MovingSpeed;
            float _MinDotSize;
            float _MaxDotSize;
            float _AlphaControl;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float2 RotateUV(float2 uv, float2 pivot, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                uv -= pivot;
                float2 rotated = float2(
                    uv.x * c - uv.y * s,
                    uv.x * s + uv.y * c
                );
                return rotated + pivot;
            }

            float ellipse(float2 uv, float2 center, float2 radius)
            {
                float2 diff = (uv - center) / radius;
                return saturate(1.0 - dot(diff, diff));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Get the aspect ratio (width / height)
                float aspect = _ScreenParams.x / _ScreenParams.y;

                // Compensate for distortion along the X axis
                uv.x *= aspect;

                // Rotate the movement vector by the grid's rotation angle
                float angleRad = radians(_RotationAngle);
                float2 rotatedSpeed = float2(
                    _MovingSpeed.x * cos(angleRad) - _MovingSpeed.y * sin(angleRad),
                    _MovingSpeed.x * sin(angleRad) + _MovingSpeed.y * cos(angleRad)
                );

                // Move UVs along the rotated direction
                uv += _Time.y * rotatedSpeed;

                // Rotate the grid
                uv = RotateUV(uv, float2(0.5, 0.5), angleRad);

                // Tile
                uv = frac(uv * _Size);

                // Rotate each point by its own angle
                uv = RotateUV(uv, float2(0.5, 0.5), radians(_DotsAngle));

                // Radius from gradient
                float t = saturate(i.uv.y);
                float curvedT = 1.0 - cos(t * 0.5 * 3.14159265); // 1 - cos(PI * t / 2)
                float radius = lerp(_MinDotSize, _MaxDotSize, curvedT);
                //float radius = lerp(_MinDotSize, _MaxDotSize, saturate(i.uv.y));

                // Mask through ellipse (point)
                float mask = ellipse(uv, float2(0.5, 0.5), float2(radius, radius));

                // Color
                float3 finalColor = lerp(_BaseColor.rgb, _DotsColor.rgb, mask);

                // Taking into account the alpha from BaseColor, the mask, and the global alpha control.
                float alpha = mask * _AlphaControl * _BaseColor.a;

                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
