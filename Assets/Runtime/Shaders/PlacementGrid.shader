Shader "Custom/PlacementGrid"
{
    Properties
    {
        _Color ("Grid Color", Color) = (1,1,1,0.3)
        _LineWidth ("Line Width", Float) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
"Queue"="Transparent" 
          "RenderType"="Transparent"
            "IgnoreProjector"="True"
 }
        
        LOD 100
   
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
   Cull Off
        Lighting Off
        
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
       float _LineWidth;
    
            v2f vert (appdata v)
            {
         v2f o;
       o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
       return o;
     }
       
   fixed4 frag (v2f i) : SV_Target
    {
             return _Color;
 }
            ENDCG
  }
    }
    
    Fallback "Sprites/Default"
}
