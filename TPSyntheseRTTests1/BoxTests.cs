using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPSyntheseRT;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace TPSyntheseRT.Tests
{
    public class BoxTests
    {
        [TestMethod()]
        public void UnionBoxTest()
        {
            Sphere sphere = new Sphere(new Vector3(0, 0, 0), 1);
            Sphere sphere2 = new Sphere(new Vector3(0, 0, 0), 2);

            Box box1 = new Box(sphere);
            Box box2 = new Box(sphere2);

            Box uBox = Box.UnionBox(new Box[] { box1, box2 });

            Assert.AreEqual(box2, uBox);

            Assert.Fail();
        }
    }
}