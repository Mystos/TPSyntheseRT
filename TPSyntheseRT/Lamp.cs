using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Lamp
    {
        public Vector3 position;
        public Vector3 le;
        public Vector3 albedo;

        public Lamp(Vector3 position, Vector3 le, Vector3 albedo)
        {
            this.position = position;
            this.le = le;
            this.albedo = albedo;
        }


    }
}
