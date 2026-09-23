Shader "SurakshaAR/SafetyHighlight"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionIntensity ("Emission Intensity", Range(0,5)) = 0
        _RiskLevel ("Risk Level", Range(0,1)) = 0
        _PulseSpeed ("Pulse Speed", Range(0,5)) = 2
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _EmissionColor;
        half _EmissionIntensity;
        half _RiskLevel;
        half _PulseSpeed;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            half pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
            half riskPulse = _RiskLevel * pulse;

            fixed4 emission = _EmissionColor * (_EmissionIntensity + riskPulse * 2);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = emission.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
