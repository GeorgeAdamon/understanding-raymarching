using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    [ExecuteAlways]
    public abstract class MarcherBase : RayMarchingPassBase
    {
        [Range(1, 256)]
        public int stepsPerRay = 16;

        [Range(0.01f, 1)]
        public float fixedStep = 0.01f;

        private NativeArray<float3> cachedEntryPoints;
        private NativeArray<float3> cachedExitPoints;

        private void Start()
        { }
    }
}