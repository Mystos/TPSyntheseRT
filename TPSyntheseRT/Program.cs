using SFML.Graphics;
using SFML.Graphics.Glsl;
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
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace TPSyntheseRT
{
    public class Program
    {
        static void Main(string[] args)
        {

            uint width = 1000;
            uint height = 1000;
            Vector3 pointPerspective = new Vector3(width / 2, height / 2, -5000);
            Image image = new Image(width, height, SFML.Graphics.Color.Cyan);

            Scene mainScene = new Scene();
            foreach(Sphere sphere in BuildSphere(width, height))
            {
                mainScene.objectsInScene.Add(sphere);
            }
            Sphere sphereCentre = new Sphere(new Vector3(width / 2, height / 2, 500), 200);
            GetMeshFromSphere(sphereCentre, 10, 10, out List<Vector3> listVerticies, out List<int> listIndexes);
            Polygone poly = new Polygone(listVerticies, listIndexes);
            mainScene.objectsInScene.Add(poly);


            //Polygone poly = new Polygone();
            //OFFReader.ReadFile(@"D:\bunny.off", out poly.listVerticies, out poly.listIndexes, 100);

            mainScene.lamps.AddRange(BuildLamp(width, height));

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

                    int raycount = 1;

                    Vector3 couleur = Vector3.Zero;

                    for (int i = 0; i < raycount; i++)
                    {
                        Ray ray = new Ray(new Position(new Vector3(x + GetRandomNumber(-offsetRay, offsetRay), y + GetRandomNumber(-offsetRay, offsetRay), 0)), dir);

                        if (CastRay(ray, mainScene, option, out Vector3 couleurPix))
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

            //for (int i = 0; i < 5; i++)
            //{
            //    Sphere sphere = new Sphere(new Vector3(GetRandomNumber(0, width), GetRandomNumber(0, height), GetRandomNumber(0, 1000)), 50);

            //    listSphere.Add(sphere);
            //}


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

        public static bool CastRay(Ray rayon, Scene mainScene, Option option, out Vector3 colorHit, uint depth = 0)
        {
            float epsilon = 0.08f;
            colorHit = new Vector3(0, 0, 0);

            if (depth > option.maxDepth)
            {
                colorHit = new Vector3(0.5f, 0.5f, 0.5f);
                return true;
            }

            if (GetFirstIntersectionInScene(mainScene, rayon, out Hit hit))
            {
                // On renvoie un rayon depuis xPos vers L
                Vector3 pEp = hit.position - epsilon * rayon.Direction.Dir;
                Vector3 N = Vector3.Zero;
                switch (hit.obj)
                {
                    case Sphere sphere:
                        N = Vector3.Normalize(pEp - hit.obj.center);
                        break;
                    case Polygone poly:
                        Vector3 u = hit.vec[1] - hit.vec[0];
                        Vector3 v = hit.vec[2] - hit.vec[0];
                        N = Vector3.Normalize(Vector3.Cross(u, v));
                        break;
                    case Box box:
                        N = Vector3.Normalize(pEp - hit.obj.center);
                        break;
                }

                if (hit.obj.type == SurfaceType.Reflective || hit.obj.type == SurfaceType.Metalic)
                {
                    Ray reflecRay = new Ray(new Position(pEp), new Direction(CalculReflection(rayon, N)));
                    if (CastRay(reflecRay, mainScene, option, out Vector3 newColor, depth + 1))
                    {
                        colorHit += 0.8f * newColor;
                    }
                }
                if (hit.obj.type == SurfaceType.Diffuse || hit.obj.type == SurfaceType.Metalic)
                {
                    Vector3 pointRandomLamp;

                    foreach (Lamp lamp in mainScene.lamps)
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

                        if (IsThereAnIntersectionBetweenAandB(pEp, pointRandomLamp, mainScene))
                        {
                            colorHit += new Vector3(0, 0, 0);
                        }
                        else
                        {
                            colorHit += CalculInstensity(1, N, Vector3.Normalize(pointRandomLamp - pEp), pEp, pointRandomLamp, lamp.le, hit.obj.albedo);
                        }
                    }
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

        public static bool IsThereAnIntersectionBetweenAandB(Vector3 a, Vector3 b, Scene mainScene)
        {
            float minDistance = float.MaxValue;
            bool foundIntersection = false;
            Ray ray = new Ray(new Position(a), new Direction(b - a));
            foreach (Object3D obj in mainScene.objectsInScene)
            {
                float distance;
                switch (obj)
                {
                    case Sphere sphere:
                        if (Intersect_Ray_Sphere(ray, sphere, out distance))
                        {
                            if (distance < minDistance && distance <= (b - a).Length())
                            {
                                minDistance = distance;
                                foundIntersection = true;

                            }
                        }
                        break;
                    case Polygone poly:
                        for (int i = 0; i < poly.listIndexes.Count; i += 3)
                        {
                            if (Intersect_Ray_Triangle_Moller(ray,
                                poly.listVerticies[poly.listIndexes[i]],
                                poly.listVerticies[poly.listIndexes[i + 1]],
                                poly.listVerticies[poly.listIndexes[i + 2]],
                                out distance))
                            {
                                if (distance < minDistance && distance <= (b - a).Length())
                                {
                                    minDistance = distance;
                                    foundIntersection = true;

                                }
                            }
                        }
                        break;
                    case Box box:
                        if (Intersect_Ray_Box(ray, box, out distance))
                        {
                            if (distance < minDistance && distance <= (b - a).Length())
                            {
                                minDistance = distance;
                                foundIntersection = true;

                            }
                        }

                        break;
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
        public static bool GetFirstIntersectionInScene(Scene mainScene, Ray rayon, out Hit firstHit)
        {
            firstHit = new Hit();
            bool hasFoundIntersection = false;

            foreach (Object3D obj in mainScene.objectsInScene)
            {
                float t;

                switch (obj)
                {
                    case Sphere sphere:
                        if (Intersect_Ray_Sphere(rayon, sphere, out t))
                        {
                            if (!hasFoundIntersection)
                            {
                                firstHit.obj = sphere;
                                firstHit.distance = t;
                            }
                            else if (t < firstHit.distance)
                            {
                                firstHit.obj = sphere;
                                firstHit.distance = t;
                            }
                            hasFoundIntersection = true;
                        }
                        break;
                    case Polygone poly:
                        for (int i = 0; i < poly.listIndexes.Count; i += 3)
                        {
                            if (Intersect_Ray_Triangle_Moller(rayon,
                                 poly.listVerticies[poly.listIndexes[i]],
                                 poly.listVerticies[poly.listIndexes[i + 1]],
                                 poly.listVerticies[poly.listIndexes[i + 2]],
                                out t))
                            {
                                if (!hasFoundIntersection)
                                {
                                    firstHit.obj = poly;
                                    firstHit.distance = t;
                                    firstHit.vec = new Vector3[] {
                                        poly.listVerticies[poly.listIndexes[i]],
                                        poly.listVerticies[poly.listIndexes[i+1]],
                                        poly.listVerticies[poly.listIndexes[i+2]]
                                        };
                                }
                                else if (t < firstHit.distance)
                                {
                                    firstHit.obj = poly;
                                    firstHit.distance = t;
                                    firstHit.vec = new Vector3[] {
                                        poly.listVerticies[poly.listIndexes[i]],
                                        poly.listVerticies[poly.listIndexes[i+1]],
                                        poly.listVerticies[poly.listIndexes[i+2]]
                                        };
                                }
                                hasFoundIntersection = true;
                            }
                        }
                        break;

                    case Box box:
                        if (Intersect_Ray_Box(rayon, box, out t))
                        {
                            if (!hasFoundIntersection)
                            {
                                firstHit.obj = box;
                                firstHit.distance = t;
                            }
                            else if (t < firstHit.distance)
                            {
                                firstHit.obj = box;
                                firstHit.distance = t;
                            }
                            hasFoundIntersection = true;
                        }
                        break;
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
            Vector3 oc = ray.StartPosition.Origin - sphere.center;
            float a = 1;
            float b = 2.0f * Vector3.Dot(oc, ray.Direction.Dir);
            float c = Vector3.Dot(oc, oc) - sphere.radius * sphere.radius;
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

        public static bool Intersect_Ray_Triangle_Moller(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
        {
            float kEpsilon = 0.1f;
            float u, v;
            t = 0;
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            Vector3 pvec = Vector3.Cross(ray.Direction.Dir, v0v2);
            float det = Vector3.Dot(v0v1,pvec);
            if (det > -kEpsilon && det < kEpsilon) return false;

            float invDet = 1 / det;
            Vector3 tvec = ray.StartPosition.Origin - v0;
            u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1) return false;

            Vector3 qvec = Vector3.Cross(tvec, v0v1);
            v = Vector3.Dot(ray.Direction.Dir, qvec) * invDet;
            if (v < 0 || u + v > 1) return false;

            t = Vector3.Dot(v0v2, qvec) * invDet;

            if(t < 0)
            {
                return false;
            }
            //Console.WriteLine(t);
            return t > 0;
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

        public static void GetMeshFromSphere(Sphere sphere, int meridian, int parallel, out List<Vector3> listVertex,out List<int> listIndex)
        {
            float x, y, z, theta, phi;

            listVertex = new List<Vector3>();

            for (int p = 0; p <= parallel; p++)
            {
                phi = p * (float)Math.PI / parallel;

                for (int m = 0; m <= meridian; m++)
                {
                    theta = m * 2 * (float)Math.PI / meridian;
                    x = sphere.radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    y = sphere.radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    z = sphere.radius * (float)Math.Cos(phi);

                    listVertex.Add(new Vector3(x + sphere.center.X, y + sphere.center.Y, z + sphere.center.Z));
                }
            }

            listIndex = GenIndices(meridian,parallel);
        }
        
        private static List<int> GenIndices(int meridian, int parallel )
        {
            List<int> indices = new List<int>();
            int k1, k2;
            for (int i = 0; i < parallel; i++)
            {
                k1 = i * (meridian + 1);
                k2 = k1 + meridian + 1;

                for (int j = 0; j < meridian; j++, k1++, k2++)
                {
                    if (i != 0)
                    {
                        indices.Add(k1);
                        indices.Add(k2);
                        indices.Add(k1 + 1);
                    }

                    if (i != parallel - 1)
                    {
                        indices.Add(k1 + 1);
                        indices.Add(k2);
                        indices.Add(k2 + 1);
                    }
                }
            }

            return indices;
        }
    }
}
