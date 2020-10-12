using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Hit
    {
        public float distance = 0;
        public Vector3 position = new Vector3(0,0,0);
        public Object3D obj;
        public Vector3[] vec;

        public Hit()
        {

        }

    }
}
