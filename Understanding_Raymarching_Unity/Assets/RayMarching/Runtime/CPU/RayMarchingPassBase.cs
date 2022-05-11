using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public abstract class RayMarchingPassBase :MonoBehaviour
    {
        public bool               shouldVisualize = true;
        
        public Texture3D          volumeAsset;
       
        [Header("Debug")] public Material mat;

        public NativeArray<float> volumeData => volumeAsset.GetPixelData<float>(0);

        protected Camera _camera;
        
        protected Mesh visMesh;

        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            _camera = Camera.main;
        }

        protected virtual void OnDrawGizmos()
        {
            if (shouldVisualize)
                Visualize();
        }

        protected abstract void Visualize();

        protected abstract void Allocate(int collectionLength);

        protected abstract void Deallocate();

        protected abstract void Execute();

        //---------------------------------------------------------------------

        
        protected virtual void OnDestroy()
        {
            Deallocate();
        }

        protected virtual void OnDisable()
        {
            Deallocate();
        }

        protected virtual void OnApplicationQuit()
        {
            Deallocate();
        }
       
     
        //---------------------------------------------------------------------

      
        public static T SampleVolume<T>(Bounds bounds, float3 xyz, NativeArray<T> volumeData, int3 dimensions)  where T :struct
        {
            var norm = math.remap(bounds.min, bounds.max, 0, 1, xyz);

            return SampleVolume(norm, volumeData, dimensions);
        }
        
        protected static T SampleVolume<T>(float3 xyz, NativeArray<T> volumeData, int3 dimensions)  where T :struct
        {
            var indexCoordinates = (int3) xyz * new int3(dimensions.x, dimensions.y, dimensions.z);

            return SampleVolume(indexCoordinates, volumeData, dimensions);
        }
        
        protected static T SampleVolume<T>(int3 xyz, NativeArray<T> volumeData, int3 dimensions)  where T :struct
        {
            var i = xyz.x + xyz.y * dimensions.x + xyz.z * dimensions.x * dimensions.y;

            return volumeData[i];
        }
    }
}