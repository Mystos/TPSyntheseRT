using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Sphere : Object3D
    {
        public readonly float radius;

        public Sphere(Vector3 center, float radius = 0)
        {
            this.center = center;
            this.radius = radius;
        }
    }


}
