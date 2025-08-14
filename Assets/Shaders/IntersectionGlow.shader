Shader "CornEngine/IntersectionGlow"
{
 Properties{
 _MainTex("MainTex",2D) = "white"{}
 _RimFactor("RimFactor",Range(0.0,5.0))=1.0
 _DistanceFactor("DistanceFactor",Range(0.0,10.0))=1.0
 _RimColor("RimColor",Color)=(1,0,0,1)
 _DistanceFactor2("DistanceFactor2",Range(0.0,10.0))=1.0
 _DistanceFactor3("DistanceFactor3",Range(0.0,5.0)) = 1.0
 }
 SubShader{
 Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector"="true"}
 Pass{
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 CGPROGRAM
 #include "UnityCG.cginc"
 #pragma vertex vert
 #pragma fragment frag

 sampler2D _MainTex;
 float4 _MainTex_ST;
 sampler2D _CameraDepthTexture;
 float _RimFactor;
 float _DistanceFactor;
 float4 _RimColor;
 float _DistanceFactor2;
 float _DistanceFactor3;

 struct a2v {
 float4 vertex:POSITION;
 float2 uv:TEXCOORD0;
 float3 normal:NORMAL;
 };

 struct v2f {
 float2 uv:TEXCOORD0;
 float4 pos:SV_POSITION;
 float4 screenPos:TEXCOORD1;
 float3 worldNormal:TEXCOORD2;
 float3 worldViewDir:TEXCOORD3;
 };

 v2f vert(a2v v) {
 v2f o;
 o.pos = UnityObjectToClipPos(v.vertex);
 //Computescreenpos function to get the normalized coordinates XY
 //Z component is Z value of clipping space, range [- near, far]
 o.screenPos = ComputeScreenPos(o.pos);
 o.uv = TRANSFORM_TEX(v.uv,_MainTex);
 //COMPUTE_ The eyedepth function converts the z-component range [- near, far] to [near, far]
 COMPUTE_EYEDEPTH(o.screenPos.z);
 o.worldNormal = UnityObjectToWorldNormal(v.normal);
 o.worldViewDir = WorldSpaceViewDir(v.vertex).xyz;
 return o;
 }

 float4 frag(v2f i):SV_Target {
 float3 mainTex = 1-tex2D(_MainTex,i.uv).xyz;
 //The depth texture is obtained, and the sampled depth texture value is converted to the corresponding depth range [near ~ far] through the lineareeyedepth function
 float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screenPos)));
 //Observe the spatial depth difference, the smaller the value, the larger the color value
 float distance =1 - saturate(sceneZ - i.screenPos.z);
 //Eliminate the sawtooth caused by large change of internal depth
 if (distance>0.999999)
 {
 distance = 0;
 }
 //Change curve of adjustment depth difference
 distance = pow(saturate(_DistanceFactor * log(distance) + _DistanceFactor3), _DistanceFactor2);

 //The larger the angle, the brighter the edge light
 float rim =1 - abs(dot(normalize(i.worldNormal), normalize(i.worldViewDir)));
 rim = pow(rim, _RimFactor);
 float4 col = float4(0,0,0,0);
 col = lerp(col, float4(_RimColor.rgb,0.3), mainTex.r);
 //Gradient according to edge light and depth difference
 col = float4(_RimColor.rgb,lerp(col.a,_RimColor.a, distance));
 col = lerp(col, _RimColor, rim);
 return col;
 }

 ENDCG
 }
 }
}
