using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Option
    {
        public uint maxDepth = 5;
        public Vector3 backgroundColor = new Vector3(Color.Cyan.R, Color.Cyan.G, Color.Cyan.B);
        public CameraType cameraType = CameraType.Normal;

        public Option()
        {

        }
    }
    public enum CameraType
    {
        Normal,
        Perspective
    }
}
