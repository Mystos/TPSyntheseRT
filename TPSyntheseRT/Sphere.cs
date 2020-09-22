using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Sphere
    {
        private Vector3 center;
        private float radius;
        private SurfaceType type;

        public Vector3 Center { get => center; set => center = value; }
        public float Radius { get => radius; set => radius = value; }
        public SurfaceType Type { get => type; set => type = value; }

        public Sphere(Vector3 center, float radius = 0, SurfaceType type = SurfaceType.Diffuse)
        {
            this.center = center;
            this.radius = radius;
            this.type = type;
        }
    }

    public enum SurfaceType
    {
        Reflective,
        Diffuse
    }
}
