using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public class RayMarcher:RayMarchingPassBase
    {
     
        public int                rayCount;
   
        protected override void Visualize()
        {
        }

        protected override void Allocate(int collectionLength)
        {
            rayCount = collectionLength;
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