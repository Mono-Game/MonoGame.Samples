//////////////////////////////////////////////////////////////////////
//                                                                  //
// Shader altered to compile on both OpenGL projects and DirectX.   //
// Pixel shader parameters passed by structure rather than by      //
// individual parameter values.                                     //
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

float4x4 WorldViewProj;
float4 LightPosition;
float3 LightColor;
float3 LightAmbient;
float3 CameraPosition;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};
sampler2D TextureSamplerClamp = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
};

texture2D Bump0;
sampler2D NormalSampler = sampler_state
{
    Texture = <Bump0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Specular0;
sampler2D SpecularSampler = sampler_state
{
    Texture = <Specular0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Emissive0;
sampler2D GlowMapSampler = sampler_state
{
    Texture = <Emissive0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

textureCUBE Reflect;
samplerCUBE ReflectSampler = sampler_state
{
    Texture = <Reflect>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};


#if OPENGL

void PlainMappingVS( 
     in float4 InPosition    : SV_POSITION,
     in float2 InTexCoord    : TEXCOORD0,
    out float4 OutPosition    : SV_POSITION,
    out float2 OutTexCoord    : TEXCOORD0 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    OutTexCoord = InTexCoord;
}

float4 PlainMappingPS( in float2 TexCoord : TEXCOORD0 ) : COLOR0
{
    return tex2D(TextureSampler, TexCoord);
}

void NormalMappingVS( 
     in float4 InPosition    : SV_POSITION,
     in float2 InTexCoord    : TEXCOORD0,
     in float3 InNormal      : NORMAL0,  
     in float3 InBinormal    : BINORMAL0,
     in float3 InTangent     : TANGENT0,
    out float4 OutPosition   : SV_POSITION,
    out float2 OutTexCoord   : TEXCOORD0,
    out float3 OutLightDir   : TEXCOORD1,
    out float3 OutViewDir    : TEXCOORD2,
    out float3 OutReflectDir : TEXCOORD3 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    
    OutTexCoord = InTexCoord;

    float3x3 tangent_space = float3x3(InTangent, InBinormal, InNormal);
    
    OutLightDir = mul(tangent_space, LightPosition.xyz - InPosition.xyz);
   
    OutViewDir = mul(tangent_space, CameraPosition - InPosition.xyz);
    
    OutReflectDir = reflect(InPosition.xyz - CameraPosition, InNormal);
}

float4 NormalMappingPS(
    in float2 TexCoord        : TEXCOORD0,
    in float3 LightDir        : TEXCOORD1,
    in float3 ViewDir         : TEXCOORD2,
    in float3 ReflectDir      : TEXCOORD3 ) : COLOR0
{
    float4 diffuse = tex2D(TextureSampler, TexCoord);
    float4 specular = tex2D(SpecularSampler, TexCoord);
    float4 normal = tex2D(NormalSampler, TexCoord);
    float4 reflect = texCUBE(ReflectSampler, ReflectDir);
    float4 glow = tex2D(GlowMapSampler, TexCoord);
    
    float3 n = normalize(normal.xyz - 0.5);
    float3 l = normalize(LightDir);
    float3 v = normalize(ViewDir);
    float3 h = normalize(l+v);
    
    float ndotl = saturate(dot(n,l));
    float ndoth = saturate(dot(n,h));
    if (ndotl == 0)    ndoth = 0;
    
    float3 ambient = LightAmbient * diffuse.xyz;
    
    specular.xyz *= LightColor * pow(ndoth, specular.w * 255);
    diffuse.xyz *= LightColor * ndotl;
    reflect *= 1 - normal.w;

    float glow_intensity = saturate(dot(glow.xyz, 1.0) + dot(specular.xyz, 1.0));

    float4 color;
    color.xyz = ambient + glow.xyz + diffuse.xyz + specular.xyz + reflect.xyz;
    color.w = glow_intensity;
    
    return color;
}

void ViewMappingVS( 
     in float4 InPosition   : SV_POSITION,
     in float3 InNormal     : NORMAL0,  
    out float4 OutPosition  : SV_POSITION,
    out float  OutFacing    : TEXCOORD0 )
{
    OutPosition = mul(InPosition, WorldViewProj);
    
    float3 view = normalize(CameraPosition - InPosition.xyz);
    
    OutFacing = saturate(dot(view, InNormal));
}

float4 ViewMappingPS(in float Facing : TEXCOORD0) : COLOR0
{
    Facing *= Facing;
    Facing *= Facing;
    
    float4 tex = tex2D(TextureSamplerClamp, float2(Facing, 0));
    tex.w = 1.0f;

    return tex;
}

#else

struct VS_INPUT_NM
{
    float4 InPosition : SV_POSITION;
    float2 InTexCoord : TEXCOORD0;
    float3 InNormal : NORMAL0;
    float3 InBinormal : BINORMAL0;
    float3 InTangent : TANGENT0;
};
struct VS_OUTPUT_NM
{
    float4 Position : SV_POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 LightDir : NORMAL0;
    float3 ViewDir : TEXCOORD1;
    float3 ReflectDir : NORMAL1;
};

struct VS_INPUT_PM
{
    float4 InPosition    : SV_POSITION;
    float2 InTexCoord    : TEXCOORD0;
    float4 OutPosition    : SV_POSITION;
    float2 OutTexCoord    : TEXCOORD0;
};

struct VS_OUTPUT_PM
{
    float4 OutPosition    : SV_POSITION;
    float2 OutTexCoord    : TEXCOORD0;
};


struct VS_INPUT_VM
{
    float4 InPosition   : SV_POSITION;
    float3 InNormal     : NORMAL0;
};

struct VS_OUTPUT_VM
{
    float4 OutPosition  : SV_POSITION;
    float  OutFacing    : TEXCOORD0;
};

VS_OUTPUT_VM ViewMappingVS(VS_INPUT_VM input)
{
    VS_OUTPUT_VM output;

    output.OutPosition = mul(input.InPosition, WorldViewProj);
    
    float3 view = normalize(CameraPosition - input.InPosition.xyz);
    
    output.OutFacing = saturate(dot(view, input.InNormal));

    return output;
}

float4 ViewMappingPS(VS_OUTPUT_VM input) : COLOR0
{
    input.OutFacing *= input.OutFacing;
    input.OutFacing *= input.OutFacing;
    
    float4 tex = tex2D(TextureSamplerClamp, float2(input.OutFacing, 0));
    tex.w = 1.0f;

    return tex;
}

VS_OUTPUT_PM PlainMappingVS(VS_INPUT_PM input )
{
    VS_OUTPUT_PM output;

    output.OutPosition = mul(input.InPosition, WorldViewProj);
    output.OutTexCoord = input.InTexCoord;

    return output;
}

float4 PlainMappingPS( VS_OUTPUT_PM input ) : COLOR0
{
    return tex2D(TextureSampler, input.OutTexCoord);
}

VS_OUTPUT_NM NormalMappingVS(VS_INPUT_NM input)
{
    VS_OUTPUT_NM Output;

    Output.Position = mul(input.InPosition, WorldViewProj);
    
    Output.TexCoord = input.InTexCoord;

    float3x3 tangent_space = float3x3(input.InTangent, input.InBinormal, input.InNormal);
    
    Output.LightDir = mul(tangent_space, LightPosition.xyz - input.InPosition.xyz);
   
    Output.ViewDir = mul(tangent_space, CameraPosition - input.InPosition.xyz);
    
    Output.ReflectDir = reflect(input.InPosition.xyz - CameraPosition, input.InNormal);

    return Output;
}

float4 NormalMappingPS(VS_OUTPUT_NM input) : COLOR0
{
    float4 diffuse = tex2D(TextureSampler, input.TexCoord);
    float4 specular = tex2D(SpecularSampler, input.TexCoord);
    float4 normal = tex2D(NormalSampler, input.TexCoord);
    float4 reflect = texCUBE(ReflectSampler, input.ReflectDir);
    float4 glow = tex2D(GlowMapSampler, input.TexCoord);
    
    float3 n = normalize(normal.xyz - .5);
    float3 l = normalize(input.LightDir);
    float3 v = normalize(input.ViewDir);
    float3 h = normalize(l+v);
    
    float ndotl = saturate(dot(n,l));
    float ndoth = saturate(dot(n,h));
    if (ndotl == 0)    ndoth = 0;
    
    float3 ambient = LightAmbient * diffuse.xyz * 1;
    
    specular.xyz *= LightColor * pow(ndoth, specular.w * 255);
    diffuse.xyz *= LightColor * ndotl;
    reflect *= 1 - normal.w;

    float glow_intensity = saturate(dot(glow.xyz, 1.0) + dot(specular.xyz, 1.0));

    float4 color;
    color.xyz = ambient + glow.xyz + diffuse.xyz + specular.xyz + reflect.xyz;
    color.w = glow_intensity;
    
    return color;
}
#endif

Technique PlainMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL PlainMappingVS();
        PixelShader = compile PS_SHADERMODEL PlainMappingPS();
    }
}

Technique NormalMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL NormalMappingVS();
        PixelShader = compile PS_SHADERMODEL NormalMappingPS();
    }
}

Technique ViewMapping
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL ViewMappingVS();
        PixelShader = compile PS_SHADERMODEL ViewMappingPS();
    }
}

