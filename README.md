# Understanding Raymarching
 
**A Unity (so far) project that attempts to demystify the beautiful world of [Volumetric Raymarching](https://en.wikipedia.org/wiki/Volume_ray_casting), by splitting a typical ray-marching pipeline in explicit steps, and executing them in parallel on the CPU (using Unity's [Job System]() & [Burst Compiler]()).**

By executing the individual steps of a RayMarching Pipeline on the CPU, instead of the usual way that is purely GPU-based, one loses performance but (hopefully) gains insight, and the ability to more closely observe, debug, and visualize what is going-on behind the scenes.  

In a sense, it's like pulling the break, and taking things slow and steady.
 
## Passes Implemented so far
- [x] **Volume Texture Proxy Generation**
- [x] **Camera Ray Generation**
- [x] **Ray-AABB Intersection**
- [ ] Ray-Marching
- [ ] Sphere-Marching
- [ ] Cone-Marching

## References
WIP
* [Sebastian Aaltonen - Making Of Claybook](https://www.youtube.com/watch?v=Xpf7Ua3UqOA)  
* [Inigo Quilez - Raymarching SDFs](https://www.iquilezles.org/www/articles/raymarchingdf/raymarchingdf.htm)  
* [Inigo Quilez - Rendering Worlds with Two Triangleswith raytracing on the GPUin 4096 bytes](https://www.iquilezles.org/www/material/nvscene2008/rwwtt.pdf)  
* [Keenan Crane - Ray Tracing Quaternion Julia Sets on the GPU](https://www.cs.cmu.edu/~kmcrane/Projects/QuaternionJulia/paper.pdf)
