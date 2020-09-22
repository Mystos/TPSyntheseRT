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
            Vector3 pointPerspective = new Vector3(width / 2, height/ 2, -500);
            Image image = new Image(width, height, SFML.Graphics.Color.Cyan);

            List<Sphere> sphereList = new List<Sphere>();
            //Sphere sphereGrosse = new Sphere(new Vector3(0, 1000000, 0), 1000000 / 2);
            //sphereList.Add(sphereGrosse);
            Sphere sphere1 = new Sphere(new Vector3(width / 2, height / 2, 400), 250, SurfaceType.Reflective);
            Sphere sphere2 = new Sphere(new Vector3(0, height / 2, 400), 250);
            sphere2.Albedo = new Vector3(1, 0, 0);
            Sphere sphere3 = new Sphere(new Vector3(width, height / 2, 400), 250);
            sphere3.Albedo = new Vector3(0, 0, 1);
            Sphere sphere4 = new Sphere(new Vector3(width / 2, 0, 400), 250);
            Sphere sphere5 = new Sphere(new Vector3(width / 2, height, 400), 250);
            sphereList.Add(sphere1);
            sphereList.Add(sphere2);
            sphereList.Add(sphere3);
            sphereList.Add(sphere4);
            sphereList.Add(sphere5);

            List<Lamp> listLamp = new List<Lamp>();
            listLamp.Add(new Lamp(new Vector3(0, 0, 200), new Vector3(1000000, 0, 0), new Vector3(1,0,0)));
            listLamp.Add(new Lamp(new Vector3(width , height, 200), new Vector3(0, 0, 1000000), new Vector3(0,0,1)));

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    Option option = new Option();
                    option.cameraType = CameraType.Perspective;
                    Direction dir = new Direction(new Vector3(0, 0, 1));

                    if (option.cameraType == CameraType.Perspective)
                    {
                        dir = new Direction(new Vector3(x, y, 0) - new Vector3(pointPerspective.X, pointPerspective.Y, pointPerspective.Z));
                    }

                    Ray ray = new Ray(new Position(new Vector3(x, y, 0)), dir);
                    Vector3 couleurPix = new Vector3(0, 0, 0);

                    couleurPix = CastRay(ray, sphereList, listLamp, option);
   
                    Color col = CreateColorFromVector(couleurPix);
                    image.SetPixel(x, y, col); // Lumiere

                }
            }
            image.SaveToFile("result.png");
        }


        public static Vector3 CastRay(Ray rayon, List<Sphere> listSphere, List<Lamp> listLamp, Option option, uint depth = 0)
        {
            float epsilon = 0.01f;
            Vector3 couleurPix = new Vector3(0, 0, 0);

            if (depth > option.maxDepth)
            {
                return option.backgroundColor;
            }

            if (GetFirstIntersectionInScene(listSphere, rayon, out Hit hit))
            {
                // On renvoie un rayon depuis xPos vers L
                Vector3 pEp = hit.position - epsilon * rayon.Direction.Dir;
                Vector3 N = Vector3.Normalize(pEp - hit.sphere.Center);

                switch (hit.sphere.Type)
                {
                    case SurfaceType.Reflective:

                        Ray reflecRay = new Ray(new Position(pEp), new Direction(CalculReflection(rayon, N)));
                        couleurPix +=  0.8f  * CastRay(reflecRay, listSphere, listLamp, option, depth + 1);
                        break;
                    case SurfaceType.Diffuse:
                            foreach (Lamp lamp in listLamp)
                            {
                                Ray rayFromX = new Ray(new Position(pEp), new Direction(lamp.position - pEp));

                                if (IsThereAnIntersectionBetweenAandB(pEp, lamp.position, listSphere))
                                {
                                    couleurPix += new Vector3(0, 0, 0);
                                }
                                else
                                {
                                    couleurPix += CalculInstensity(1, N, Vector3.Normalize(lamp.position - pEp), pEp, lamp.position, lamp.le, hit.sphere.Albedo);
                                }
                            }
                            break;
                }
            }
            else
            {
                return option.backgroundColor;
            }

            return couleurPix;
        }

        public static Vector3 CalculReflection(Ray ray, Vector3 N)
        {
            return (Vector3.Dot(-ray.Direction.Dir, N)) * N * 2 + ray.Direction.Dir; 
        }

        public static bool IsThereAnIntersectionBetweenAandB(Vector3 a, Vector3 b, List<Sphere> scene)
        {
            Ray ray = new Ray(new Position(a), new Direction(b-a));
            foreach (Sphere sphere in scene)
            {
                if (Intersect_Ray_Sphere(ray, sphere, out _))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Donne la premiere intersection
        /// </summary>
        /// <param name="listSphere"></param>
        /// <param name="rayon"></param>
        /// <returns></returns>
        public static bool GetFirstIntersectionInScene(List<Sphere> listSphere, Ray rayon, out Hit firstHit)
        {
            firstHit = new Hit();
            bool hasFoundIntersection = false;
            foreach (Sphere sphere in listSphere)
            {
                float t;
                if (Intersect_Ray_Sphere(rayon, sphere, out t))
                {
                    if(!hasFoundIntersection)
                    {
                        firstHit.sphere = sphere;
                        firstHit.distance = t;
                    }
                    else if(t < firstHit.distance)
                    {
                        firstHit.sphere = sphere;
                        firstHit.distance = t;
                    }
                    hasFoundIntersection = true;
                }
            }
            if (hasFoundIntersection)
            {
                // On recupere la position de x
                firstHit.position = rayon.StartPosition.Origin + firstHit.distance * rayon.Direction.Dir;
            }

            return hasFoundIntersection;


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
        /// Calcule l'intensit� lumineuse d'une surface non mirroir
        /// </summary>
        /// <param name="V">la visibilit� entre x et y (i.e. est-ce que x voit y sans obstacle, i.e. 0 ou 1)</param>
        /// <param name="N">N c'est la normal � la surface</param>
        /// <param name="L">L c'est le vecteur en direction de la lampe (i.e. XY normalis�)</param>
        /// <param name="x">Point x</param>
        /// <param name="y">Point y</param>
        /// <param name="LQ">La quantit� de lumi�re �mise par la lampe dans la direction du point �clair�e</param>
        /// <param name="A">Alb�do, ou couleur de la surface. Pour une surface rouge (1, 0, 0)</param>
        /// <returns>Un vecteur contenant l'intensit� de la lumiere</returns>
        public static Vector3 CalculInstensity(float V, Vector3 N, Vector3 L, Vector3 x, Vector3 y, Vector3 LQ,Vector3 A)
        {
            float cosT = Math.Abs(Vector3.Dot(N, L)); // Vector are normalized : magnitude equal 1
            float dist2 = Vector3.Distance(x, y) * Vector3.Distance(x, y);
            return V * cosT * LQ * A/ (dist2 * (float)Math.PI);
        }
    }
}
