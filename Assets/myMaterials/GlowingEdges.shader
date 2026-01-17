Shader "Custom/GlowingEdgesFresnel"
{
    Properties
    {
        // צבע הקצוות הזוהרים
        [MainColor] _BaseColor("Glow Color", Color) = (0, 1, 0, 1)
        
        // עוצמת החיתוך - ככל שהמספר גבוה יותר, הקו יהיה דק יותר
        _FresnelPower("Fresnel Power", Range(0.5, 10.0)) = 3.0
    }

    SubShader
    {
        // הגדרות שקיפות - קריטי כדי שיראו רק את הקצוות
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        // מצב ערבוב שיוצר שקיפות
        Blend SrcAlpha OneMinusSrcAlpha
        // מבטל כתיבה לעומק כדי שהחלק האחורי ייראה דרך הקדמי (אפקט הולוגרמה)
        ZWrite Off 

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; // הוספנו את הנורמלים כדי לחשב זווית
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1; // מיקום בעולם
                float3 normalWS : TEXCOORD3;   // נורמל בעולם
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _FresnelPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // חישוב מיקום רגיל
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                
                // חישוב מיקום ונורמל בעולם האמיתי (World Space)
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. נרמול וקטורים (כדי שהחישובים יהיו מדויקים)
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));

                // 2. חישוב אפקט הפרנל (Fresnel)
                // הנוסחה בודקת את הזווית בין המצלמה למשטח
                // דוט פרודקט (Dot Product) נותן 1 כשהם מסתכלים זה על זה, ו-0 בקצוות
                float fresnel = dot(normal, viewDir);
                
                // הופכים את התוצאה (1 פחות) כדי שהקצוות יהיו 1 (לבן/זוהר) והמרכז 0 (שחור/שקוף)
                fresnel = saturate(1.0 - fresnel);

                // 3. חידוד הקו
                // מעלים בחזקה כדי שהמעבר יהיה חד יותר ופחות מרוח
                fresnel = pow(fresnel, _FresnelPower);

                // 4. החלת הצבע
                // לוקחים את הצבע שבחרת וכופלים בעוצמת הפרנל
                half4 finalColor = _BaseColor;
                
                // קובעים את השקיפות (Alpha) לפי הפרנל
                finalColor.a = fresnel * _BaseColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}