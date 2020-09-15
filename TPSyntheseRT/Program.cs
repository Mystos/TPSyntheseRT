using SFML.Graphics;
using System;
using System.Numerics;
using System.Xml;

namespace TPSyntheseRT
{
    class Program
    {
        static void Main(string[] args)
        {
            uint width = 1000;
            uint height = 1000;

            Image image = new Image(width, height, Color.Black);

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    Ray ray = new Ray(new Position(new Vector3(x, y, 0)), new Direction(new Vector3(0, 0, 1)));
                    Sphere sphere = new Sphere(new Vector3(width/2, height/2, 10), 500);
                    float discriminant = 0;
                    if(Intersect_Ray_Sphere(ray, sphere, ref discriminant))
                    {
                        image.SetPixel(x, y, Color.White);
                    }
                }
            }
            image.SaveToFile("result.png");
        }

        public static bool Intersect_Ray_Sphere(Ray ray, Sphere sphere, ref float discriminant)
        {
            Vector3 oc = ray.StartPosition.Origin - sphere.Center;
            float a = Vector3.Dot(ray.Direction.Dir, ray.Direction.Dir);
            float b = 2.0f * Vector3.Dot(oc, ray.Direction.Dir);
            float c = Vector3.Dot(oc, oc) - sphere.Radius * sphere.Radius;
            discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


    }
}
