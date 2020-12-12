using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using UnityEngine;


namespace RayMarching.Runtime.CPU
{
    public static class Utils
    {
        /// <summary>
        /// http://answers.unity.com/answers/1695591/view.html
        /// </summary>
        public static Ray ViewportPointToRay(float2 point, float4x4 projMatrix, float4x4 worldToCamMatrix)
        {
            var m    = mul(projMatrix , worldToCamMatrix);
            var mInv = inverse(m); 
       
            // near clipping plane point
            var p  = new float4(point.x * 2 - 1, point.y * 2 - 1, -1, 1);
            var p0 = mul(mInv , p);
            p0 /= p0.w;
        
            // far clipping plane point
            p.z = 1;
            var p1 =  mul(mInv , p);
            p1 /= p1.w;
      
            return new Ray(p0.xyz, (p1 - p0).xyz);
        }
    
        /// <summary>
        /// http://answers.unity.com/answers/1695591/view.html
        /// </summary>
        public static float3 ViewportPointToWorld(float2 point, float4x4 projMatrix, float4x4 worldToCamMatrix)
        {
            var m    = mul(projMatrix , worldToCamMatrix);
            var mInv = inverse(m); 
        
            // near clipping plane point
            var p  = new float4(point.x *2 -1, point.y *2 -1, -1, 1);
            var p0 =  mul(mInv , p);
            p0 /= p0.w;
        
            return p0.xyz;
        }
        
        /// <summary>
        /// https://medium.com/@bromanz/another-view-on-the-classic-ray-aabb-intersection-algorithm-for-bvh-traversal-41125138b525
        /// </summary>
        public static bool RayAABBIntersection(Ray ray, Bounds aabb, out float tmin, out float tmax)
        {
            var invD = 1f                      / (float3) ray.direction.normalized;
            var t0s  = (aabb.min - ray.origin) * invD;
            var t1s  = (aabb.max - ray.origin) * invD;

            var tsmaller = min(t0s, t1s);
            var tbigger  = max(t0s, t1s);

            tmin = max(tsmaller.x,max(tsmaller.y, tsmaller.z));
            tmax = min(tbigger.x, min(tbigger.y, tbigger.z));

            return tmin < tmax;
        }


    }
}