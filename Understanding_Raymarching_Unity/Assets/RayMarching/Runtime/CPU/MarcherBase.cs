using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    [ExecuteAlways]
    public abstract class MarcherBase : RayMarchingPassBase
    {
        [Range(1, 256)]
        public int maxStepsPerRay = 16;

        [Range(0.01f, 1)]
        public float fixedStep = 0.01f;

        [Header("Read-Only Info")]
        public int rayCount;


        // --------------------------------------------------------------------
        // READ-ONLY REFERENCES
        protected NativeArray<float3> rayEntryPoints;
        protected NativeArray<float3> rayExitPoints;
        protected NativeArray<bool>   rayHitInfo;

        protected override void Allocate(int collectionLength)
        {
            rayCount = collectionLength;
        }

        public void OnIntersectionResultsReceived(NativeArray<float3> entry, NativeArray<float3> exit, NativeArray<bool> results)
        {
            Allocate(entry.Length);
            
            rayEntryPoints = entry;
            
            rayExitPoints = exit;

            rayHitInfo = results;
            
            Execute();
        }
    }
}