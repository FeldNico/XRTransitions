﻿Shader "Custom/Transform"
{
    Properties
    {
        _TransformMask ("Texture", 2D) = "white" {}
        _LeftEyeTexture ("Texture", 2D) = "white" {}
        _RightEyeTexture("Texture", 2D) = "white" {}
        _Alpha ("Transparency", Float) = 1
        _Count ("Count", Int) =30
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 screenPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _TransformMask;
            sampler2D _LeftEyeTexture;
            sampler2D _RightEyeTexture;
            float _Alpha;
            int _Count;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex); // use the screen position coordinates of the portal to sample the render texture (which is our screen)
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                const float2 screenUV = i.screenPos.xy / i.screenPos.w; // clip space -> normalized texture
 
                // sample the texture
                const float2 uv = i.uv * _Count;
                
                fixed4 col = (unity_StereoEyeIndex == 0 ? tex2D(_LeftEyeTexture, screenUV) : tex2D(_RightEyeTexture, screenUV)) * (1,1,1,clamp(_Alpha * 2 - tex2D(_TransformMask,uv)[0],0,1)) ;
 
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col );
                return col;
            }
            ENDCG
        }
    }
}