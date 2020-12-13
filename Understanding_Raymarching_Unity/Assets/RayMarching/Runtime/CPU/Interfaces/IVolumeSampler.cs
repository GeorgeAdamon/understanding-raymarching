using System;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU.Interfaces
{
    public interface IVolumeSampler
    {
        float4 Sample(int3 xyz);
       
        Bounds SampleBox(int3 xyz);
        
        float3 Min { get; }
        
        float3 Max { get; }
        
        int3 Resolution { get; }
    }
}