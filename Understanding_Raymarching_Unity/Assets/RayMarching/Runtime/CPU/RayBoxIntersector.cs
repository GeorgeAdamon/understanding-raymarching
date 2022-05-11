using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace RayMarching.Runtime.CPU
{
    [ExecuteAlways]
    public class RayBoxIntersector : RayMarchingPassBase
    {
        [Serializable]
        public class CameraRaysIntersectionEvent:UnityEvent<NativeArray<float3>, NativeArray<float3>, NativeArray<bool>>
        { }

        public BoxCollider                 box;
        public CameraRaysIntersectionEvent OnRaysAABBIntersection;
        
        private NativeArray<Ray>    cachedRays;
        private NativeArray<float3> boxEntryPoints;
        private NativeArray<float3> boxExitPoints;
        private NativeArray<bool>   boxIntersectionResults;
        
        public void OnRaysReceived(NativeArray<Ray> rays)
        {
            cachedRays = rays;
           
            Allocate(cachedRays.Length);
            
            Execute();
        }

        
        protected override void Allocate(int rayCount)
        {
            boxEntryPoints         = new NativeArray<float3>(rayCount, Allocator.Persistent);
            boxExitPoints          = new NativeArray<float3>(rayCount, Allocator.Persistent);
            boxIntersectionResults = new NativeArray<bool>(rayCount, Allocator.Persistent);
        }

        protected override void Deallocate()
        {
            if (boxEntryPoints.IsCreated)
                boxEntryPoints.Dispose();

            if (boxExitPoints.IsCreated)
                boxExitPoints.Dispose();

            if (boxIntersectionResults.IsCreated)
                boxIntersectionResults.Dispose();
        }

        protected override void Execute()
        {
            new IntersectBoxJob
            {
                    entryPts = boxEntryPoints,
                    exitPts  = boxExitPoints,
                    rays     = cachedRays,
                    results  = boxIntersectionResults,
                    bounds   = box.bounds
            }.Schedule(cachedRays.Length, 32).Complete();
            
            OnRaysAABBIntersection.Invoke(boxEntryPoints, boxExitPoints, boxIntersectionResults);
        }
        
        protected override void Visualize()
        {
            if (!boxIntersectionResults.IsCreated)
                return;

            for (var i = 0; i < boxIntersectionResults.Length; i++)
            {
                if (boxIntersectionResults[i] == false)
                    continue;

                Gizmos.color = Color.black;
                Gizmos.DrawSphere(boxEntryPoints[i], 0.02f);
                Gizmos.DrawSphere(boxExitPoints[i],  0.02f);
            }

            if (!boxIntersectionResults.IsCreated) 
                return;

            for (var i = 0; i < boxIntersectionResults.Length; i++)
            {
                if (boxIntersectionResults[i] == false)
                    continue;

                Debug.DrawLine(boxEntryPoints[i], boxExitPoints[i], new Color(0.3f,0.2f,0.8f,0.7f));
            }
           
        }



        [BurstCompile]
        private struct RayMarchJob : IJobParallelFor
        {
            public NativeArray<Ray>    rays;
            public NativeArray<float3> startPts;
            public NativeArray<float3> endPts;
            public int                 steps;

            public void Execute(int index)
            {
                float3 p = startPts[index];

                var step = math.length(startPts[index] - endPts[index]) / steps;

                for (int i = 0; i < steps; i++)
                {
                    p += (float3) rays[index].direction.normalized * step;
                }

                endPts[index] = p;
            }
        }

        [BurstCompile]
        private struct IntersectBoxJob : IJobParallelFor
        {
            public NativeArray<Ray>    rays;
            public NativeArray<float3> entryPts;
            public NativeArray<float3> exitPts;
            public NativeArray<bool>   results;
            public Bounds              bounds;

            public void Execute(int index)
            {
                results[index]  = Utils.RayAABBIntersection(rays[index], bounds, out var start, out var end);
                entryPts[index] = (float3) rays[index].origin + (float3) rays[index].direction.normalized * start;
                exitPts[index]  = (float3) rays[index].origin + (float3) rays[index].direction.normalized * end;
            }
        }
    }
}