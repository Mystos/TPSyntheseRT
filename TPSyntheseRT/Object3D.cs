using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public abstract class Object3D
    {
        public SurfaceType type = SurfaceType.Diffuse;
        public Vector3 albedo = new Vector3(1, 1, 1);
        public Vector3 center = Vector3.Zero;


    }

    public enum SurfaceType
    {
        Reflective,
        Diffuse,
        Metalic
    }
}
