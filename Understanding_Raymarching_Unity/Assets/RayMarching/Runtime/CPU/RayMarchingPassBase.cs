using System;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public abstract class RayMarchingPassBase :MonoBehaviour
    {
        public bool shouldVisualize = true;

        protected virtual void OnDrawGizmos()
        {
            if (shouldVisualize)
                Visualize();
        }

        protected abstract void Visualize();

        protected abstract void Allocate(int collectionLength);

        protected abstract void DeAllocate();

        protected abstract void Execute();

        protected virtual void OnDestroy()
        {
            DeAllocate();
        }

        protected virtual void OnDisable()
        {
            DeAllocate();
        }

        protected virtual void OnApplicationQuit()
        {
            DeAllocate();
        }
    }
}