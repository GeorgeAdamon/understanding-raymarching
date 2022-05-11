using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace RayMarching.Runtime.CPU
{
    public class RayMarcher : MarcherBase
    {
        public enum MarchingMethod
        {
            Classic,
            SnapToPlanes
        }

        // --------------------------------------------------------------------
        // SERIALIZED FIELDS
        [Header("Algorithm Properties")] public MarchingMethod method;

        [Range(0f, 1f)] public float jitter;

        // --------------------------------------------------------------------
        // WRITABLE ARRAYS - ALLOCATED HERE
        private NativeArray<float3> interSectionPoints;
        private NativeArray<int>    intersectionsPerRay;

        // --------------------------------------------------------------------
        // LOCAL FIELDS
        private Plane   _viewPlane;
        private Vector3 _closestRayOrigin;

        protected override void Visualize()
        {
            if (method != MarchingMethod.SnapToPlanes)
                return;

            Gizmos.matrix = Matrix4x4.TRS(_closestRayOrigin, Quaternion.LookRotation(_viewPlane.normal), Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(8, 8, 0));
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawSphere(_closestRayOrigin, 0.1f);



        }

        protected override void Allocate(int collectionLength)
        {
            base.Allocate(collectionLength);

            if (interSectionPoints.IsCreated)
                interSectionPoints.Dispose();

            if (intersectionsPerRay.IsCreated)
                intersectionsPerRay.Dispose();

            interSectionPoints  = new NativeArray<float3>(collectionLength * maxStepsPerRay, Allocator.Persistent);
            intersectionsPerRay = new NativeArray<int>(collectionLength, Allocator.Persistent);
        }

        protected override void Deallocate()
        {
            if (interSectionPoints.IsCreated)
                interSectionPoints.Dispose();

            if (intersectionsPerRay.IsCreated)
                intersectionsPerRay.Dispose();
        }

        protected override void Execute()
        {
            if (method == MarchingMethod.SnapToPlanes)
            {
                if (_camera == null)
                    return;

                // Get Near Plane
                _viewPlane = GeometryUtility.CalculateFrustumPlanes(Camera.main)[4];

                // Find Starting plane position
                var minDist = Mathf.Infinity;
                _closestRayOrigin = _camera.transform.position;

                for (var i = 0; i < rayEntryPoints.Length; i++)
                {
                    if (rayHitInfo[i] == false)
                        continue;

                    var dist = Vector3.Distance(rayEntryPoints[i], _camera.transform.position);

                    if (!(dist < minDist))
                        continue;

                    minDist           = dist;
                    _closestRayOrigin = rayEntryPoints[i];
                }

                _viewPlane = new Plane(-_viewPlane.normal, _closestRayOrigin);

                new RayMarchingJob_Planes
                {
                        entryPoints         = rayEntryPoints,
                        exitPoints          = rayExitPoints,
                        results             = rayHitInfo,
                        interSectionPoints  = interSectionPoints,
                        intersectionsPerRay = intersectionsPerRay,
                        maxSteps            = maxStepsPerRay,
                        step                = fixedStep,
                        plane               = _viewPlane,
                        jitter              = jitter,
                        origDist            = _viewPlane.distance
                }.Schedule(rayEntryPoints.Length, 32).Complete();

            }
            else
            {
                new RayMarchingJob_Classic
                {
                        entryPoints         = rayEntryPoints,
                        exitPoints          = rayExitPoints,
                        results             = rayHitInfo,
                        interSectionPoints  = interSectionPoints,
                        intersectionsPerRay = intersectionsPerRay,
                        maxSteps            = maxStepsPerRay,
                        step                = fixedStep,
                        jitter              = jitter,
                }.Schedule(rayEntryPoints.Length, 32).Complete();
            }

            if (mat != null && shouldVisualize)
            {
                if (visMesh == null)
                    visMesh = new Mesh();
                

                    visMesh.Clear();
                    visMesh.indexFormat = IndexFormat.UInt32;
                    visMesh.SetVertices(interSectionPoints);

                    var indices = new List<int>();

                    for (var i = 0; i < rayCount; i++)
                    {
                        var intersections = intersectionsPerRay[i];

                        for (var j = 0; j < intersections; j++)
                        {
                            indices.Add(i * maxStepsPerRay + j);
                        }
                    }
                    
                    visMesh.SetIndices(indices, MeshTopology.Points, 0, true);

                Graphics.DrawMesh(visMesh, Matrix4x4.identity, mat, 0);
            }

        }


        [BurstCompile]
        private struct RayMarchingJob_Planes : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> entryPoints;
            [ReadOnly] public NativeArray<float3> exitPoints;
            [ReadOnly] public NativeArray<bool>   results;

            [WriteOnly] [NativeDisableParallelForRestriction]
            public NativeArray<float3> interSectionPoints;

            [WriteOnly] public NativeArray<int> intersectionsPerRay;

            public int   maxSteps;
            public float step;
            public Plane plane;
            public float origDist;
            public float jitter;

            public void Execute(int i)
            {
                intersectionsPerRay[i] = 0;

                if (results[i] == false)
                    return;

                plane.distance = origDist;

                // Create Ray
                var dir = exitPoints[i] - entryPoints[i];
                var ray = new Ray(entryPoints[i], dir);

                var tan  = math.cross(math.normalize(dir), new float3(0, 1, 0));
                var tan2 = math.cross(math.normalize(dir), tan);
                var rand = Random.CreateFromIndex((uint)i);

                var rayLength  = math.length(dir);
                var cnt        = 0;
                var startIndex = i * maxSteps;

                for (var j = 0; j < maxSteps; j++)
                {
                    var randX = rand.NextFloat(-jitter, jitter);
                    var randY = rand.NextFloat(-jitter, jitter);

                    if (plane.Raycast(ray, out var enter))
                    {
                        if (enter > 0 && enter <= rayLength)
                        {
                            interSectionPoints[startIndex + cnt] =
                                    (float3)ray.GetPoint(enter) + tan * randX + tan2 * randY;
                            cnt++;
                        }
                    }

                    plane.distance += step;
                }

                intersectionsPerRay[i] = cnt;
            }
        }

        [BurstCompile]
        private struct RayMarchingJob_Classic : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> entryPoints;
            [ReadOnly] public NativeArray<float3> exitPoints;
            [ReadOnly] public NativeArray<bool>   results;

            [WriteOnly] [NativeDisableParallelForRestriction]
            public NativeArray<float3> interSectionPoints;

            [WriteOnly] public NativeArray<int> intersectionsPerRay;

            public int   maxSteps;
            public float step;
            public float jitter;

            public void Execute(int i)
            {
                intersectionsPerRay[i] = 0;

                if (results[i] == false)
                    return;

                // Create Ray
                var dir = exitPoints[i] - entryPoints[i];
                var ray = new Ray(entryPoints[i], dir);

                var tan  = math.cross(math.normalize(dir), new float3(0, 1, 0));
                var tan2 = math.cross(math.normalize(dir), tan);
                var rand = Random.CreateFromIndex((uint)i);

                var rayLength  = math.length(dir);
                var dist       = 0f;
                var cnt        = 0;
                var startIndex = i * maxSteps;

                for (var j = 0; j < maxSteps; j++)
                {
                    if (dist > rayLength)
                        break;

                    var randX = rand.NextFloat(-jitter, jitter);
                    var randY = rand.NextFloat(-jitter, jitter);

                    interSectionPoints[startIndex + cnt] =  (float3)ray.GetPoint(dist) + tan * randX + tan2 * randY;
                    dist                                 += step;
                    cnt++;
                }

                intersectionsPerRay[i] = cnt;
            }
        }
    }
}