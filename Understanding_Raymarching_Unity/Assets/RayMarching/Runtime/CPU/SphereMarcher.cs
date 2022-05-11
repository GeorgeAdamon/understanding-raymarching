using Unity.Collections;
using Unity.Mathematics;
    using UnityEngine;

namespace RayMarching.Runtime.CPU
{
    public class SphereMarcher:MarcherBase
    {
        [Range(0,3)]
        public float radius;
       
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