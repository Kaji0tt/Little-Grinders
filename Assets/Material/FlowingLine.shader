Shader "Custom/FlowingLine"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FlowSpeed ("Flow Speed", Float) = 1
        _FlowTiling ("Flow Tiling", Float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
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
            
            float4 _Color;
            float _FlowSpeed;
            float _FlowTiling;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float flow = frac(i.uv.x * _FlowTiling - _Time.y * _FlowSpeed);
                float alpha = smoothstep(0.0, 0.3, flow) * smoothstep(1.0, 0.7, flow);
                
                fixed4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}