using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using System.Xml;


namespace TPSyntheseRT
{
    public class Program
    {
        static void Main(string[] args)
        {

            uint width = 1000;
            uint height = 1000;
            Image image = new Image(width, height, SFML.Graphics.Color.Cyan);

            List<Sphere> sphereList = new List<Sphere>();
            sphereList.Add(new Sphere(new Vector3(0, height / 2, 400), 200));
            sphereList.Add(new Sphere(new Vector3(width, height / 2, 400), 200));
            sphereList.Add(new Sphere(new Vector3(width /2, 0, 400), 200));
            sphereList.Add(new Sphere(new Vector3(width /2, height, 400), 200));

            List<Lamp> listLamp = new List<Lamp>();
            listLamp.Add(new Lamp(new Vector3(width / 2, height / 2 + 50, 200), new Vector3(1000000, 1000000, 1000000), new Vector3(1,0,0)));
            listLamp.Add(new Lamp(new Vector3(width / 2, height / 2 - 50, 200), new Vector3(1000000, 1000000, 1000000), new Vector3(0,1,0)));


            float epsilon = 0.01f;

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    Ray ray = new Ray(new Position(new Vector3(x, y, 0)), new Direction(new Vector3(0, 0, 1)));
                    float t;

                    foreach(Sphere sphere in sphereList)
                    {
                        if (Intersect_Ray_Sphere(ray, sphere, out t))
                        {
                            // On recupere la position de x
                            Vector3 xPos = ray.StartPosition.Origin + t * ray.Direction.Dir;
                            // On renvoie un rayon depuis xPos vers L
                            Vector3 Pep = xPos - epsilon * ray.Direction.Dir;

                            Vector3 couleurPix = new Vector3(0,0,0);
                            foreach (Lamp lamp in listLamp)
                            {
                                Ray rayFromX = new Ray(new Position(Pep), new Direction(Vector3.Normalize(lamp.position - Pep)));
                                float tL;
                                if (Intersect_Ray_Sphere(rayFromX, sphere, out tL))
                                {
                                    image.SetPixel(x, y, Color.Black);// Ombre
                                }
                                else
                                {
                                    Vector3 N = Vector3.Normalize(Pep - sphere.Center);
                                    couleurPix += CalculInstensity(1, N, Vector3.Normalize(lamp.position - Pep), Pep, lamp.position, lamp.le, lamp.albedo);
                                    Color col = CreateColorFromVector(couleurPix);
                                    image.SetPixel(x, y, col); // Lumiere
                                }
                            }
                        }
                    }
                }
            }
            image.SaveToFile("result.png");
        }
        public static Color CreateColorFromVector(Vector3 vectColor)
        {
            float maxIntensity = 2;
 
            int r = Math.Clamp((int)(Math.Pow(vectColor.X, 1 / 2.2) * 255f / maxIntensity), 0,255);
            int g = Math.Clamp((int)(Math.Pow(vectColor.Y, 1 / 2.2) * 255f / maxIntensity), 0, 255);
            int b = Math.Clamp((int)(Math.Pow(vectColor.Z, 1 / 2.2) * 255f / maxIntensity), 0, 255);
            return new Color((byte)r, (byte)g, (byte)b, 255);
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

        /// <summary>
        /// Calcule l'intensité lumineuse d'une surface non mirroir
        /// </summary>
        /// <param name="V">la visibilité entre x et y (i.e. est-ce que x voit y sans obstacle, i.e. 0 ou 1)</param>
        /// <param name="N">N c'est la normal à la surface</param>
        /// <param name="L">L c'est le vecteur en direction de la lampe (i.e. XY normalisé)</param>
        /// <param name="x">Point x</param>
        /// <param name="y">Point y</param>
        /// <param name="LQ">La quantité de lumière émise par la lampe dans la direction du point éclairée</param>
        /// <param name="A">Albédo, ou couleur de la surface. Pour une surface rouge (1, 0, 0)</param>
        /// <returns>Un vecteur contenant l'intensité de la lumiere</returns>
        public static Vector3 CalculInstensity(float V, Vector3 N, Vector3 L, Vector3 x, Vector3 y, Vector3 LQ,Vector3 A)
        {
            float cosT = Math.Abs(Vector3.Dot(N, L)); // Vector are normalized : magnitude equal 1
            float dist2 = Vector3.Distance(x, y) * Vector3.Distance(x, y);
            return V * cosT * LQ * A/ (dist2 * (float)Math.PI);
        }
    }
}
