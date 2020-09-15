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

        public Vector3 Center { get => center; set => center = value; }
        public float Radius { get => radius; set => radius = value; }

        public Sphere(Vector3 center, float radius = 0)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
