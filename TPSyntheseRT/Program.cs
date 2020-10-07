using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;

namespace TPSyntheseRT
{
    public class Program
    {
        static void Main(string[] args)
        {

            uint width = 1000;
            uint height = 1000;
            Vector3 pointPerspective = new Vector3(width / 2, height / 2, -500);
            Image image = new Image(width, height, SFML.Graphics.Color.Cyan);

            List<Sphere> sphereList = BuildSphere(width, height);


            OFFReader.ReadFile(@"D:\bunny.off", out List<Vector3> listVertex, out List<int> listIndex);

            List<Lamp> listLamp = BuildLamp(width, height);

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    Option option = new Option();
                    //option.backgroundColor = new Vector3(0, 0, 0);
                    option.cameraType = CameraType.Perspective;
                    option.maxDepth = 5;
                    Direction dir = new Direction(new Vector3(0, 0, 1));
                    float offsetRay = 0.5f;
                    if (option.cameraType == CameraType.Perspective)
                    {
                        dir = new Direction(new Vector3(x, y, 0) - new Vector3(pointPerspective.X, pointPerspective.Y, pointPerspective.Z));
                    }

                    int raycount = 10;

                    Vector3 couleur = Vector3.Zero;

                    for (int i = 0; i < raycount; i++)
                    {
                        Ray ray = new Ray(new Position(new Vector3(x + GetRandomNumber(-offsetRay, offsetRay), y + GetRandomNumber(-offsetRay, offsetRay), 0)), dir);

                        if (CastRay(ray, sphereList, listLamp, option, out Vector3 couleurPix))
                        {

                            couleur += couleurPix;

                        }
                        else
                        {
                            couleur += option.backgroundColor;

                        }
                    }

                    Color col = CreateColorFromVector(couleur / raycount);
                    image.SetPixel(x, y, col); // Lumiere
                }
            }
            image.SaveToFile(@"D:\result.png");
        }


        ///////////////////////////////////////// RANDOM OPERATIONS /////////////////////////////////////////
        private static System.Random random = new System.Random(4587);
        public static float GetRandomNumber(float minimum, float maximum)
        {
            return (float)random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static List<Sphere> BuildSphere(uint width, uint height)
        {
            List<Sphere> listSphere = new List<Sphere>();

            Sphere sphereGrosse = new Sphere(new Vector3(width / 2, 10000 + height, 800), 10000);
            listSphere.Add(sphereGrosse);

            Sphere sphereMur1 = new Sphere(new Vector3(width / 2, height / 2, 10000 + 1000), 10000);
            listSphere.Add(sphereMur1);

            Sphere sphereMur2 = new Sphere(new Vector3(width + 10000, height / 2, 0), 10000);
            listSphere.Add(sphereMur2);

            for (int i = 0; i < 5; i++)
            {
                Sphere sphere = new Sphere(new Vector3(GetRandomNumber(0,width), GetRandomNumber(0, height), GetRandomNumber(0, 1000)), 10);
                
                listSphere.Add(sphere);
            }


            return listSphere;
        }

        public static List<Lamp> BuildLamp(uint width, uint height)
        {
            List<Lamp> listLamp = new List<Lamp>();

            listLamp.Add(new Lamp(new Vector3(0, 0, 800), new Vector3(1000000, 0, 0)));
            listLamp.Add(new Lamp(new Vector3(width / 2, 0, 500), new Vector3(1000000, 1000000, 1000000)));
            listLamp.Add(new Lamp(new Vector3(width - 100, 0, 200), new Vector3(0, 0, 1000000)));

            return listLamp;
        }

        public static bool CastRay(Ray rayon, List<Sphere> listSphere, List<Lamp> listLamp, Option option, out Vector3 colorHit, uint depth = 0)
        {
            float epsilon = 0.08f;
            colorHit = new Vector3(0, 0, 0);

            if (depth > option.maxDepth)
            {
                colorHit = new Vector3(0.5f, 0.5f, 0.5f);
                return true;
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
                        if (CastRay(reflecRay, listSphere, listLamp, option, out Vector3 newColor, depth + 1))
                        {
                            colorHit += 0.8f * newColor;
                        }
                        break;
                    case SurfaceType.Diffuse:

                        Vector3 pointRandomLamp;

                        foreach (Lamp lamp in listLamp)
                        {
                            if (lamp.radius > 0)
                            {
                                Vector3 offset = new Vector3(GetRandomNumber(-lamp.radius, lamp.radius), GetRandomNumber(-lamp.radius, lamp.radius), GetRandomNumber(-lamp.radius, lamp.radius));
                                pointRandomLamp = lamp.position + offset;
                            }
                            else
                            {
                                pointRandomLamp = lamp.position;
                            }

                            Ray rayFromX = new Ray(new Position(pEp), new Direction(pointRandomLamp - pEp));

                            if (IsThereAnIntersectionBetweenAandB(pEp, pointRandomLamp, listSphere))
                            {
                                colorHit += new Vector3(0, 0, 0);
                            }
                            else
                            {
                                colorHit += CalculInstensity(1, N, Vector3.Normalize(pointRandomLamp - pEp), pEp, pointRandomLamp, lamp.le, hit.sphere.Albedo);
                            }
                        }
                        break;

                    case SurfaceType.Metalic:
                        Ray reflecRayMetal = new Ray(new Position(pEp), new Direction(CalculReflection(rayon, N)));
                        if (CastRay(reflecRayMetal, listSphere, listLamp, option, out Vector3 newColor2, depth + 1))
                        {
                            colorHit += 0.8f * newColor2;
                        }

                        Vector3 pointRandomLampMetal;

                        foreach (Lamp lamp in listLamp)
                        {

                            if (lamp.radius > 0)
                            {
                                Vector3 offset = new Vector3(GetRandomNumber(-lamp.radius, lamp.radius), GetRandomNumber(-lamp.radius, lamp.radius), GetRandomNumber(-lamp.radius, lamp.radius));
                                pointRandomLampMetal = lamp.position + offset;
                            }
                            else
                            {
                                pointRandomLampMetal = lamp.position;
                            }

                            Ray rayFromX = new Ray(new Position(pEp), new Direction(pointRandomLampMetal - pEp));

                            if (IsThereAnIntersectionBetweenAandB(pEp, pointRandomLampMetal, listSphere))
                            {
                                colorHit += new Vector3(0, 0, 0);
                            }
                            else
                            {
                                colorHit += CalculInstensity(1, N, Vector3.Normalize(pointRandomLampMetal - pEp), pEp, pointRandomLampMetal, lamp.le, hit.sphere.Albedo);
                            }
                        }

                        break;
                }
                return true;

            }
            else
            {
                return false;
            }

        }

        public static Vector3 CalculReflection(Ray ray, Vector3 N)
        {
            return (Vector3.Dot(-ray.Direction.Dir, N)) * N * 2 + ray.Direction.Dir;
        }

        public static bool IsThereAnIntersectionBetweenAandB(Vector3 a, Vector3 b, List<Sphere> scene)
        {
            float minDistance = float.MaxValue;
            bool foundIntersection = false;
            Ray ray = new Ray(new Position(a), new Direction(b - a));
            foreach (Sphere sphere in scene)
            {
                if (Intersect_Ray_Sphere(ray, sphere, out float distance))
                {
                    if(distance < minDistance && distance <= (b-a).Length())
                    {
                        minDistance = distance;
                        foundIntersection = true;

                    }
                }
            }
            return foundIntersection;
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
                    if (!hasFoundIntersection)
                    {
                        firstHit.sphere = sphere;
                        firstHit.distance = t;
                    }
                    else if (t < firstHit.distance)
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

            int r = Math.Clamp((int)(Math.Pow(vectColor.X, 1 / 2.2) * 255f / maxIntensity), 0, 255);
            int g = Math.Clamp((int)(Math.Pow(vectColor.Y, 1 / 2.2) * 255f / maxIntensity), 0, 255);
            int b = Math.Clamp((int)(Math.Pow(vectColor.Z, 1 / 2.2) * 255f / maxIntensity), 0, 255);
            return new Color((byte)r, (byte)g, (byte)b, 255);
        }

        public static bool SolveQuadratic(ref float a, ref float b, ref float c, out float x0, out float x1)
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
                x0 = (-b - MathF.Sqrt(discr)) / (2.0f * a);
                x1 = (-b + MathF.Sqrt(discr)) / (2.0f * a);
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
            else if (t < 0 && t1 > 0)
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

        public static bool Intersect_Ray_Box(Ray ray, Box box, out float t)
        {
            float t1 = (box.start.X - ray.StartPosition.Origin.X) * ray.InvDirection.Dir.X;
            float t2 = (box.end.X - ray.StartPosition.Origin.X) * ray.InvDirection.Dir.X;
            float t3 = (box.start.Y - ray.StartPosition.Origin.Y) * ray.InvDirection.Dir.Y;
            float t4 = (box.end.Y - ray.StartPosition.Origin.Y) * ray.InvDirection.Dir.Y;
            float t5 = (box.start.Z - ray.StartPosition.Origin.Z) * ray.InvDirection.Dir.Z;
            float t6 = (box.end.Z - ray.StartPosition.Origin.Z) * ray.InvDirection.Dir.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax   = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            tmin = Math.Max(tmin, 0f);
            t = tmin;

            if (tmax > tmin)
            {
                return true;
            }

            return false;

        }

        public static bool Intersect_Ray_Triangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
        {
            float kEpsilon = 0.01f;

            // Compute Plane Normal
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            // No need to normalize
            Vector3 N = Vector3.Cross(v0v1, v0v2);
            float area2 = N.Length();

            // Step 1 : Finding P

            // Check if ray and plane are parallel
            float NdotRayDirection = Vector3.Dot(N, ray.Direction.Dir);
            if (Math.Abs(NdotRayDirection) < kEpsilon) // allmost 0
            {
                t = 0;
                return false; // they are parallel so they don't intersect !
            }

            // compute d parameter using equation 2
            float d = Vector3.Dot(N, v0);

            // Compute t (equation 3)
            t = (Vector3.Dot(N, ray.StartPosition.Origin) + d) / NdotRayDirection;
            //Check if the triangle is in behind the ray
            if (t < 0) return false; // the triangle is behind

            // Compute the intersection point using eqution 1
            Vector3 P = ray.StartPosition.Origin + t * ray.Direction.Dir;

            // Step 2: inside-outside test
            Vector3 C; // Vector perpendicular to triangle's plane

            //Edge 0
            Vector3 edge0 = v1 - v0;
            Vector3 vp0 = P - v0;
            C = Vector3.Cross(edge0, vp0);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side

            //Edge 1
            Vector3 edge1 = v2 - v1;
            Vector3 vp1 = P - v1;
            C = Vector3.Cross(edge1, vp1);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side

            //Edge 2
            Vector3 edge2 = v0 - v2;
            Vector3 vp2 = P - v2;
            C = Vector3.Cross(edge2, vp2);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side

            return true;
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
        public static Vector3 CalculInstensity(float V, Vector3 N, Vector3 L, Vector3 x, Vector3 y, Vector3 LQ, Vector3 A)
        {
            float cosT = Math.Abs(Vector3.Dot(N, L)); // Vector are normalized : magnitude equal 1
            float dist2 = Vector3.Distance(x, y) * Vector3.Distance(x, y);
            return V * cosT * LQ * A / (dist2 * (float)Math.PI);
        }

    }
}
