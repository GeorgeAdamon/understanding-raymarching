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
    
    [Range(8,512)]
    public int resolutionX = 32;
    public  RaysCalculatedEvent OnRaysCalculated;
    
    private int    resolutionY => (int) (resolutionX / _camera.aspect);

    private int RayCount => resolutionX * resolutionY;
    
    private NativeArray<Ray> rays;
 

    private void Update()
    {
        if (_camera == null)
            return;
   
        Allocate(RayCount);
        
        Execute();
    }

    protected override void Allocate(int rayCount)
    {
        if (rays.IsCreated && rayCount == rays.Length)
            return;
        
        Deallocate();
        
        rays = new NativeArray<Ray>(rayCount, Allocator.Persistent); 
    }

    protected override void Deallocate()
    {
        if (rays.IsCreated)
            rays.Dispose();
    }
    
    protected override void Execute()
    {
        new CalculateRays
                {
                        rays                =  rays,
                        worldToCameraMatrix = _camera.worldToCameraMatrix,
                        projectionMatrix    = _camera.projectionMatrix,
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
            Debug.DrawRay(rays[i].origin, rays[i].direction * (_camera.farClipPlane -_camera.nearClipPlane) , new Color(0.7f,0.45f,0.2f,1));
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
