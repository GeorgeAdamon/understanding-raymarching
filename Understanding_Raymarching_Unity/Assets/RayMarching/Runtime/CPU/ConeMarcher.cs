using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public class ConeMarcher:MarcherBase
    {
        [Range(0, 45)]
        public float angle;
       
        protected override void Visualize()
        { }

        protected override void Allocate(int collectionLength)
        {
            base.Allocate(collectionLength);
        }

        protected override void Deallocate()
        { }

        protected override void Execute()
        { }
    }
}