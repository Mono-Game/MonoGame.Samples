//////////////////////////////////////////////////////////////////////
//                                                                  //
// Shader altered to compile on both OpenGL projects and DirectX.   //
// C.Humphrey  2024-02-19                                           //
//                                                                  //
//////////////////////////////////////////////////////////////////////

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 ViewProj;
float4 FrameOffset;
float2 FrameSize;
float2 FrameBlend;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

struct VS_INPUT
{
    float4 InPosition : SV_POSITION;
    float2 InTexCoord : TEXCOORD0;    
};

struct VS_OUTPUT
{
    float4 OutPosition : SV_POSITION;
    float2 OutTexCoord : TEXCOORD0;
};

struct PS_INPUT
{
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT AnimSpriteVS(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.OutPosition = mul(input.InPosition, ViewProj);
    output.OutTexCoord = input.InTexCoord;
        
    return output;
 }

float4 AnimSpritePS(VS_OUTPUT input) : COLOR0
{
    float2 tx1 = FrameSize * (FrameOffset.xy + input.OutTexCoord);
    float2 tx2 = FrameSize * (FrameOffset.zw + input.OutTexCoord);
    
    float4 color1 = tex2D(TextureSampler, tx1);
    float4 color2 = tex2D(TextureSampler, tx2);
    
    float4 blend_color = lerp(color1, color2, FrameBlend.x);
    blend_color.w *= FrameBlend.y;
    
    return blend_color;

}

Technique AnimSprite
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL AnimSpriteVS();
        PixelShader = compile PS_SHADERMODEL AnimSpritePS();
    }
}

