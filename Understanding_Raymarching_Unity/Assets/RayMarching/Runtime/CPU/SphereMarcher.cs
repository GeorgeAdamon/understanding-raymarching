using Unity.Collections;
using Unity.Mathematics;
    using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public class SphereMarcher:RayMarchingPassBase
    {
        [Range(0,3)]
        public float radius;
       
        protected override void Visualize()
        {
        }

        protected override void Allocate(int collectionLength)
        {
        }

        protected override void DeAllocate()
        {
        }

        protected override void Execute()
        {
        }
        
        public void OnIntersectionResultsReceived(NativeArray<float3> entry, NativeArray<float3> exit)
        {
            Allocate(entry.Length);

            Execute();
        }
    }
}