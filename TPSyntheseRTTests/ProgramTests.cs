using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPSyntheseRT;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Numerics;

namespace TPSyntheseRT.Tests
{
    public class ProgramTests
    {
        [TestMethod()]
        public void Intersect_Ray_SphereTest()
        {
            // Test tire depuis le centre
            Sphere sphere = new Sphere(new Vector3(0,0,10), 5);
            Position pos = new Position(Vector3.Zero);
            Direction dir = new Direction(Vector3.UnitZ);
            Ray ray = new Ray(pos, dir);
            float t;
            Assert.IsTrue(Program.Intersect_Ray_Sphere(ray, sphere, out t));
        }
    }
}