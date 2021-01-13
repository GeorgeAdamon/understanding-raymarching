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

        public Texture3D volumeAsset;
        
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
        private void OnEnable()
        {
            bounds = GetComponent<BoxCollider>();
        }

        private void OnValidate()
        {
            Allocate(VoxelCount);
            
            Execute();
        }
     
        
        // RAYMARCHINGPASSBASE -----------------------------------------------------------------------------------------
        protected override void Allocate(int voxelCount)
        {
            if (voxelsBuffer.IsCreated && voxelCount == voxelsBuffer.Length)
                return;
        
            DeAllocate();
           
            voxelsBuffer = new NativeArray<Bounds>(voxelCount, Allocator.Persistent);
        }

        protected override void DeAllocate()
        {
            if (voxelsBuffer.IsCreated)
                voxelsBuffer.Dispose();
        }
    
        protected override void Execute()
        {
            new GenerateVoxelsJob
            {
                    voxels = voxelsBuffer,
                    size   = VoxelSize,
                    count  = Resolution,
                    offset = bounds.bounds.min
            }.Schedule(voxelsBuffer.Length, 128).Complete();
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
    }
}