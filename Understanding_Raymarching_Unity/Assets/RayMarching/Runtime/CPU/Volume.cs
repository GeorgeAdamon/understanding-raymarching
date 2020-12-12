using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public class Volume : RayMarchingPassBase
    {
        [Range(8, 128)]
        public int resolution;
        
        
        private BoxCollider bounds;
        
        private NativeArray<Bounds> voxelsBuffer;

        
        private float VoxelSize => MaxSide / resolution;

        private int3 Count => (int3) math.ceil(resolution * RelativeDimensions);
        
        private int VoxelCount
        {
            get
            {
                var cnt = Count;
                return cnt.x * cnt.y * cnt.z;
            }
        }
        
        private float MaxSide => math.max(bounds.bounds.size.z, math.max(bounds.bounds.size.x, bounds.bounds.size.y));

        private float3 RelativeDimensions => bounds.bounds.size / MaxSide;


        private void OnEnable()
        {
            bounds = GetComponent<BoxCollider>();
        }

        private void OnValidate()
        {
            Allocate(VoxelCount);
            
            Execute();
        }

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
                    count  = Count,
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