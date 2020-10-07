using SFML.Graphics.Glsl;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    class Polygone : Object3D
    {
        public readonly List<Vector3> listVerticies = new List<Vector3>(); 
        public readonly List<int> listIndexes = new List<int>();

        
        public Polygone(List<Vector3> listVerticies, List<int> listIndexes)
        {
            this.listVerticies = listVerticies;
            this.listIndexes = listIndexes;
            this.center = GetGravityCenter(listVerticies);
        }

        public static Vector3 GetGravityCenter(List<Vector3> listVertex)
        {
            Vector3 cg = Vector3.Zero;
            foreach (Vector3 vec in listVertex)
            {
                cg += vec;
            }
            return cg / listVertex.Count;
        }
    }
}
