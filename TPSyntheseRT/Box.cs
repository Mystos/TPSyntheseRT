using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    public class Box
    {
        Vector3 start;
        Vector3 end;

        public Box(Vector3 _start, Vector3 _end)
        {
            this.start = _start;
            this.end = _end;
        }

        public Box(Sphere sphere)
        {
            this.start = Vector3.Min(sphere.Center - new Vector3(sphere.Radius, sphere.Radius, sphere.Radius), start);
            this.end = Vector3.Max(sphere.Center + new Vector3(sphere.Radius, sphere.Radius, sphere.Radius), end);
        }

        public static Box UnionBox(IList<Box> listBox)
        {
            if (listBox.Count <= 0)
            {
                return null;
            }
            Vector3 start = listBox[0].start;
            Vector3 end = listBox[0].end;

            foreach (Box box in listBox)
            {
                start = Vector3.Min(box.start, start);
                end = Vector3.Max(box.end, end);
            }

            return new Box(start, end);

        }

        public static Box UnionBox(List<Sphere> listSphere)
        {
            if (listSphere.Count <= 0)
            {
                return null;
            }
            Vector3 start = listSphere[0].Center;
            Vector3 end = listSphere[0].Center;

            foreach (Sphere sphere in listSphere)
            {
                start = Vector3.Min(sphere.Center - new Vector3(sphere.Radius, sphere.Radius, sphere.Radius), start);
                end = Vector3.Max(sphere.Center + new Vector3(sphere.Radius, sphere.Radius, sphere.Radius), end);
            }

            Vector3 diag = end - start;

            //// Permet de créer le cube englobant
            //float maxLength = Mathf.Max(diag.x, diag.y, diag.z);
            //end.x += maxLength - diag.x;
            //end.y += maxLength - diag.y;
            //end.z += maxLength - diag.z;
            ////////////////////////////////////////////

            return new Box(start, end);
        }

        public override bool Equals(object obj)
        {
            Box box = obj as Box;
            if (box == null)
                return false;
            else
                return base.Equals(box.start == this.start && box.end == this.end);
        }
    }
}
