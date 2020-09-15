using SFML.Graphics;
using System;
using System.ComponentModel.DataAnnotations;
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
            Sphere sphere = new Sphere(new Vector3(width / 2, height / 2, 400), 200);
            Vector3 L = sphere.Center + Vector3.UnitY * (sphere.Radius + 100);
            float epsilon = 0.1f;

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    Ray ray = new Ray(new Position(new Vector3(x, y, 0)), new Direction(new Vector3(0, 0, 1)));
                    float t;
                    if (Intersect_Ray_Sphere(ray, sphere, out t))
                    {
                        // On recupere la position de x
                        Vector3 xPos = ray.StartPosition.Origin + t * ray.Direction.Dir;
                        // On renvoie un rayon depuis xPos vers L
                        Vector3 Pep = xPos - epsilon * ray.Direction.Dir;
                        Ray rayFromX = new Ray(new Position(Pep), new Direction(Vector3.Normalize(L - Pep)));
                        float tL;
                        if (Intersect_Ray_Sphere(rayFromX, sphere, out tL))
                        {
                            //byte color = (byte)(tL / (L - sphere.Center).Length() * 255);  
                            //image.SetPixel(x, y, new Color(color, color, color));// Lumiere
                            image.SetPixel(x, y, Color.Black);// Ombre
                        }
                        else
                        {
                            image.SetPixel(x, y, Color.White); // Lumiere
                        }
                    }
                }
            }
            image.SaveToFile("result.png");
        }

        public static bool SolveQuadratic(ref float a,ref  float b,ref float c,out float x0,out float x1)
        {
            float discr = b * b - 4 * a * c;
            if (discr < 0)
            {
                x0 = 0; // SOLUTION A MODIFIER POUR OPTIONAL
                x1 = 0; // SOLUTION A MODIFIER POUR OPTIONAL
                return false; // Si il n'y as pas d'intersection
            }
            else
            {
                x0 = (-b - MathF.Sqrt(discr)) / (2.0f*a);
                x1 = (-b + MathF.Sqrt(discr)) / (2.0f*a);
            }

            return true;
            

        }

        public static bool Intersect_Ray_Sphere(Ray ray, Sphere sphere, out float t)
        {
            Vector3 oc = ray.StartPosition.Origin - sphere.Center;
            float a = 1;
            float b = 2.0f * Vector3.Dot(oc, ray.Direction.Dir);
            float c = Vector3.Dot(oc, oc) - sphere.Radius * sphere.Radius;
            float t1;
            bool result = SolveQuadratic(ref a, ref b, ref c, out t, out t1);
            if (t > 0)
            {
                t = t;
            }
            else if(t < 0 && t1 > 0)
            {
                t = t1;
            }
            else
            {
                t = 0;
                result = false;
            }
            return result;
        }


    }
}
