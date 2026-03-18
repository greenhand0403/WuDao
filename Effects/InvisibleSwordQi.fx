sampler SceneTexture : register(s0);
sampler MaskTexture  : register(s1);

float uTime;
float uDistortStrength;
float2 uScreenSize;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float4 scene = tex2D(SceneTexture, uv);
    float4 maskTex = tex2D(MaskTexture, uv);

    float mask = maskTex.a;
    if (mask <= 0.001)
        return scene;

    // 更像热浪的局部扰动
    float wave1 = sin(uv.y * 140.0 + uTime * 7.0);
    float wave2 = cos(uv.x * 90.0 - uTime * 5.0);
    float wave3 = sin((uv.x + uv.y) * 70.0 + uTime * 6.0);

    float2 dir = float2(1.0, 0.25);
    float2 offset = dir * (wave1 * 0.5 + wave2 * 0.3 + wave3 * 0.2) * uDistortStrength * mask;

    float4 distorted = tex2D(SceneTexture, uv + offset);

    // 只返回扭曲后的场景，不叠白色，不发光
    return lerp(scene, distorted, saturate(mask));
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}