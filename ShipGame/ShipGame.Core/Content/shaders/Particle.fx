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

float4x4 WorldViewProj;

float4 StartColor = float4(1,0,0,1);    // start color and opacity
float4 EndColor = float4(1,1,0,0);      // end color and opacity

float2 PointSize;        // min and max point sizes
float VelocityScale;     // velocity multiplier

float3 Times;            // elapsed time and particle time

#define ElapsedTime      Times.x
#define ParticleTime     Times.y
#define TotalTime        Times.z
#define ParticleSize     InTexCoord.x
#define ParticleOffset   InTexCoord.y

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    AddressU = clamp;
    AddressV = clamp;
};

struct VS_INPUT
{
    float4 InPosition : SV_POSITION;
    float3 InVelocity : NORMAL;
    float2 InTexCoord : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 OutPosition : SV_POSITION;
    float4 OutColor : COLOR0;
    float OutSize : PSIZE;
    float4 OutRotation : COLOR1;
#ifdef XBOX
    float2 TexCoord  : SPRITETEXCOORD;
#else    
    float2 TexCoord : TEXCOORD0;
#endif
};

VS_OUTPUT ParticleVS(VS_INPUT input)
{
    VS_OUTPUT output;
    
    // particle time
    float time = ElapsedTime + input.InTexCoord.y * ParticleTime;
    
    output.TexCoord = input.InTexCoord;

    // particle position
    float4 Pos = input.InPosition;
    if (time < 0) // if not yet alive move far away
        Pos.xyz = 1e10;
    
    // particle time (may loop)
    time = fmod(time, ParticleTime);
    
    // normalized particle time
    float norm_time = time / ParticleTime;
    
    // length of velocity
    float vel_len = length(input.InVelocity);
    
    // itegrate movement
    float integral = vel_len * (norm_time - 0.5 * norm_time * norm_time);
    
    // normalized velocity
    float3 norm_vel = normalize(input.InVelocity);
    
    // compute final particle position                     
    Pos.xyz += VelocityScale * norm_vel * integral * ParticleTime;

    // output position
    output.OutPosition = mul(Pos, WorldViewProj);
    
    // compute color inerpolation position
    float color_factor = 1.0 - (1.0 - norm_time)*VelocityScale;
    
    // output color
    output.OutColor = lerp(StartColor, EndColor, color_factor);
    
    // project velocity in view space and peoject in XY plane
    float2 screen_vel = mul(norm_vel, (float3x3)WorldViewProj).xy;
    
    // scaling factor for projected angle
    float angle_scale = length(screen_vel);
    
    // normalize screen velocity
    screen_vel = normalize(screen_vel);
    
    // compute 2x2 rotation matrix
    float4 rot = float4(screen_vel.y, -screen_vel.x, screen_vel.x, screen_vel.y);
    
    // output rotation
    output.OutRotation = rot * 0.5 + 0.5;

    // compute size
    float size = lerp(PointSize.x, PointSize.y, input.InTexCoord.x);
    
    // if in a burst mode (not loop mode) scale particles with angle
    if (TotalTime == ParticleTime)
        size *= angle_scale;
    
    // output size
    output.OutSize = size / output.OutPosition.w * 360;
    
    return output;
}

float4 ParticlePS(VS_OUTPUT input) : COLOR0
{
    // unpack rotation matrix
    input.OutRotation = input.OutRotation * 2 - 1;

    // rotate point sprite texcoord
    float2 tc = 0.5 + mul(input.TexCoord - 0.5, float2x2(input.OutRotation));

    // return final color
    return input.OutColor * tex2D(TextureSampler, tc);
}

Technique Particle
{
    Pass
    {
        VertexShader = compile VS_SHADERMODEL ParticleVS();
        PixelShader = compile PS_SHADERMODEL ParticlePS();
    }
}

