using System;
using System.Collections;
using System.Collections.Generic;
using RayMarching.Runtime.CPU;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class RayShooter : RayMarchingPassBase
{
    [Serializable]
    public class RaysCalculatedEvent:UnityEvent<NativeArray<Ray>>{}
    
    [Range(8,128)]
    public int resolutionX = 32;
    public  RaysCalculatedEvent OnRaysCalculated;

    private Camera cam;
    
    private int    resolutionY => (int) (resolutionX / cam.aspect);

    private int RayCount => resolutionX * resolutionY;
    
    private NativeArray<Ray> rays;
 
    
    private void OnEnable()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null)
            return;
   
        Allocate(RayCount);
        
        Execute();
    }

    protected override void Allocate(int rayCount)
    {
        if (rays.IsCreated && rayCount == rays.Length)
            return;
        
        DeAllocate();
        
        rays = new NativeArray<Ray>(rayCount, Allocator.Persistent); 
    }

    protected override void DeAllocate()
    {
        if (rays.IsCreated)
            rays.Dispose();
    }
    
    protected override void Execute()
    {
        new CalculateRays
                {
                        rays                =  rays,
                        worldToCameraMatrix = cam.worldToCameraMatrix,
                        projectionMatrix    = cam.projectionMatrix,
                        count               =  new int2(resolutionX, resolutionY),
                }.Schedule(rays.Length, 128)
                 .Complete();
     

        OnRaysCalculated.Invoke(rays);
    }
    
    protected override void Visualize()
    {
        if (!rays.IsCreated)
            return;
        
        for (var i = 0; i < rays.Length; i++)
        {
            Debug.DrawRay(rays[i].origin, rays[i].direction * (cam.farClipPlane -cam.nearClipPlane) , new Color(0.7f,0.45f,0.2f,1));
        }
    }



    [BurstCompile]
    private struct CalculateRays : IJobParallelFor
    {
        public NativeArray<Ray> rays;

        public float4x4 worldToCameraMatrix;
        public float4x4 projectionMatrix;

        public int2 count;

        public float nearPlane;
        
        public void Execute(int index)
        {
            var ix = index % count.x;
            var iy = index / count.x;

            var x = ix / (float)count.x ;
            var y = iy / (float)count.y;

            rays[index] = Utils.ViewportPointToRay(new float2(x, y), projectionMatrix, worldToCameraMatrix);
        }
    }
}
