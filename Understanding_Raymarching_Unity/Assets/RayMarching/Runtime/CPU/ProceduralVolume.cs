using System;
using RayMarching.Runtime.CPU.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public class ProceduralVolume : RayMarchingPassBase, IVolumeSampler
    {
        [Range(8, 128)]
        public int resolution;

        private int prev_resolution;
        
        // PRIVATE FIELDS ----------------------------------------------------------------------------------------------
        private BoxCollider bounds;
        
        private NativeArray<Bounds> voxelsBuffer;

        public float3 Min => bounds.bounds.min;

        public float3 Max => bounds.bounds.max;

        public int3 Resolution => (int3) math.ceil(resolution * RelativeDimensions);


        // PRIVATE PROPERTIES ------------------------------------------------------------------------------------------
        private float VoxelSize => MaxSide / resolution;

        
        private int VoxelCount
        {
            get
            {
                var cnt = Resolution;
                return cnt.x * cnt.y * cnt.z;
            }
        }
        
        private float MaxSide => math.max(bounds.bounds.size.z, math.max(bounds.bounds.size.x, bounds.bounds.size.y));

        private float3 RelativeDimensions => bounds.bounds.size / MaxSide;

        
        // MONOBEHAVIOUR -----------------------------------------------------------------------------------------------
        protected override void OnEnable()
        {
            base.OnEnable();
            bounds          = GetComponent<BoxCollider>();
            prev_resolution = 0;
        }
        
        private void Update()
        {
            if (prev_resolution == resolution) return;
            
            if (bounds == null) return;
            
            Allocate(VoxelCount);
            
            Execute();

            prev_resolution = resolution;
        }

        // RAYMARCHINGPASSBASE -----------------------------------------------------------------------------------------
        protected override void Allocate(int voxelCount)
        {
            if (voxelsBuffer.IsCreated && voxelCount == voxelsBuffer.Length)
                return;
        
            Deallocate();
            
            var res = Resolution;
            volumeAsset  = new Texture3D(res.x, res.y, res.z, TextureFormat.RFloat, false);
            voxelsBuffer = new NativeArray<Bounds>(voxelCount, Allocator.Persistent);
        }

        protected override void Deallocate()
        {
            if (voxelsBuffer.IsCreated)
                voxelsBuffer.Dispose();
        }
    
        protected override void Execute()
        {
           var handle = new GenerateVoxelsJob
            {
                    voxels = voxelsBuffer,
                    size   = VoxelSize,
                    count  = Resolution,
                    offset = bounds.bounds.min
            }.Schedule(voxelsBuffer.Length, 128);


           new PopulateVolumeJob()
           {
                   voxels = voxelsBuffer,
                   density = volumeData
           }  .Schedule(voxelsBuffer.Length,128,handle).Complete();
           
           volumeAsset.Apply();
           
           mat.SetTexture("_Volume",volumeAsset);
           mat.SetVector("_BoundsMin", bounds.bounds.min);
           mat.SetVector("_BoundsMax", bounds.bounds.max);

        }
        
        protected override void Visualize()
        {
            if (voxelsBuffer.IsCreated == false)
                return;

            Gizmos.color =new Color(0.5f,0.5f,0.5f,0.5f);
            
            for (var i = 0; i < voxelsBuffer.Length; i++)
            {
                Gizmos.DrawWireCube(voxelsBuffer[i].center, voxelsBuffer[i].size);
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        // IVOLUMESAMPLER ----------------------------------------------------------------------------------------------
        public float4 Sample(int3 xyz)
        {
            int i = xyz.x + xyz.y * Resolution.x + xyz.z * Resolution.x * Resolution.y;
            var a= voxelsBuffer[i];
            return math.length(a.center);
        }
        
        public Bounds SampleBox(int3 xyz)
        {
            int i = xyz.x + xyz.y * Resolution.x + xyz.z * Resolution.x * Resolution.y;
            return  voxelsBuffer[i];
          
        }
        
        // JOBS --------------------------------------------------------------------------------------------------------
        [BurstCompile]
        private struct GenerateVoxelsJob : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<Bounds> voxels;

            public float  size;
            public int3   count;
            public float3 offset;

            public void Execute(int index)
            {
                var ix  = index             % count.x;
                var iy  = (index / count.x) % count.y;
                var iz  = index             / (count.x * count.y);
                var pos = new float3(ix, iy, iz) * size + offset + size * 0.5f;

                voxels[index] = new Bounds(pos, new float3(size, size, size));
            }
        }

        [BurstCompile]
        private struct PopulateVolumeJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Bounds> voxels;

            [WriteOnly]
            public NativeArray<float> density;
            
            public void Execute(int index)
            {
                var noise = math.saturate(Unity.Mathematics.noise.snoise(voxels[index].center * 0.5f));
                density[index] = noise;
            }
        }
    }
}