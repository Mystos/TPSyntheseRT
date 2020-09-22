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
        public Vector3 albedo = new Vector3(1,1,1);

        public Lamp(Vector3 position, Vector3 le)
        {
            this.position = position;
            this.le = le;
        }


    }
}
