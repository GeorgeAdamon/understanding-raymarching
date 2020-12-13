using System;
using System.Collections;
using System.Collections.Generic;
using RayMarching.Runtime.CPU;
using RayMarching.Runtime.CPU.Interfaces;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class VolumePointSampler : MonoBehaviour
{
    
    public ProceduralVolume volume;

    public float4 value;

    public int3 XYZ;

    public Bounds box;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (volume == null)
            return;

        float3 pos        = transform.position;
        var    resolution = volume.Resolution;
        
        XYZ = (int3) math.remap(volume.Min, volume.Max, 0, resolution, pos);

        if (XYZ.x < 0 || XYZ.y < 0 || XYZ.z < 0)
        {
            XYZ   = -1;
            value = -1;
            return;
        }

        if (XYZ.x >= resolution.x || XYZ.y >= resolution.y || XYZ.z >= resolution.z)
        {
            XYZ   = -1;
            value = -1;
            return;
        }
        
        value = volume.Sample(XYZ);
        box   = volume.SampleBox(XYZ);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = (Vector4) new float4(math.remap(0,10,0,1,value.xxx),1);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
