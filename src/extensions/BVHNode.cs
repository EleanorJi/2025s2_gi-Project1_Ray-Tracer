using System;
using System.Collections.Generic;

namespace RayTracer
{
    public class BVHNode
    {
        public Vector3 Min;
        public Vector3 Max;
        public BVHNode Left;
        public BVHNode Right;
        public List<Triangle> Triangles;

        private const int maxTrianglesPerLeaf = 8;
        private const int maxDepth = 32;

        public BVHNode(List<Triangle> tris, int depth = 0)
        {
            BuildNode(tris, depth);
        }

        private void BuildNode(List<Triangle> tris, int depth)
        {
            if (tris == null || tris.Count == 0)
            {
                Min = new Vector3(0, 0, 0);
                Max = new Vector3(0, 0, 0);
                Triangles = new List<Triangle>();
                return;
            }

            // Calculate the bounding box
            double minX = double.PositiveInfinity, minY = double.PositiveInfinity, minZ = double.PositiveInfinity;
            double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity, maxZ = double.NegativeInfinity;

            foreach (var tri in tris)
            {
                // get vertex information from Triangle
                UpdateMinMax(tri.V0, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                UpdateMinMax(tri.V1, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                UpdateMinMax(tri.V2, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
            }

            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);

            // Termination condition
            if (tris.Count <= maxTrianglesPerLeaf || depth >= maxDepth)
            {
                Triangles = tris;
                Left = Right = null;
                return;
            }

            // Select the longest axis for division
            Vector3 extent = Max - Min;
            int axis = 0;
            if (extent.Y > extent.X && extent.Y >= extent.Z) axis = 1;
            else if (extent.Z > extent.X && extent.Z > extent.Y) axis = 2;

            // Sort by center of mass
            tris.Sort((a, b) =>
            {
                Vector3 centroidA = (a.V0 + a.V1 + a.V2) / 3.0;
                Vector3 centroidB = (b.V0 + b.V1 + b.V2) / 3.0;

                if (axis == 0) return centroidA.X.CompareTo(centroidB.X);
                if (axis == 1) return centroidA.Y.CompareTo(centroidB.Y);
                return centroidA.Z.CompareTo(centroidB.Z);
            });

            int mid = tris.Count / 2;
            var leftList = tris.GetRange(0, mid);
            var rightList = tris.GetRange(mid, tris.Count - mid);

            if (leftList.Count == 0 || rightList.Count == 0)
            {
                Triangles = tris;
                Left = Right = null;
                return;
            }

            Left = new BVHNode(leftList, depth + 1);
            Right = new BVHNode(rightList, depth + 1);
            Triangles = null;
        }

        private static void UpdateMinMax(Vector3 v, ref double minX, ref double minY, ref double minZ, ref double maxX, ref double maxY, ref double maxZ)
        {
            if (v.X < minX) minX = v.X;
            if (v.Y < minY) minY = v.Y;
            if (v.Z < minZ) minZ = v.Z;
            if (v.X > maxX) maxX = v.X;
            if (v.Y > maxY) maxY = v.Y;
            if (v.Z > maxZ) maxZ = v.Z;
        }

        public bool IntersectAABB(Ray ray)
        {
            double tmin = double.NegativeInfinity;
            double tmax = double.PositiveInfinity;

            // X
            if (Math.Abs(ray.Direction.X) < 1e-12)
            {
                if (ray.Origin.X < Min.X || ray.Origin.X > Max.X) return false;
            }
            else
            {
                double inv = 1.0 / ray.Direction.X;
                double t1 = (Min.X - ray.Origin.X) * inv;
                double t2 = (Max.X - ray.Origin.X) * inv;
                if (t1 > t2) { var tmp = t1; t1 = t2; t2 = tmp; }
                if (t1 > tmin) tmin = t1;
                if (t2 < tmax) tmax = t2;
                if (tmin > tmax) return false;
            }

            // Y
            if (Math.Abs(ray.Direction.Y) < 1e-12)
            {
                if (ray.Origin.Y < Min.Y || ray.Origin.Y > Max.Y) return false;
            }
            else
            {
                double inv = 1.0 / ray.Direction.Y;
                double t1 = (Min.Y - ray.Origin.Y) * inv;
                double t2 = (Max.Y - ray.Origin.Y) * inv;
                if (t1 > t2) { var tmp = t1; t1 = t2; t2 = tmp; }
                if (t1 > tmin) tmin = t1;
                if (t2 < tmax) tmax = t2;
                if (tmin > tmax) return false;
            }

            // Z
            if (Math.Abs(ray.Direction.Z) < 1e-12)
            {
                if (ray.Origin.Z < Min.Z || ray.Origin.Z > Max.Z) return false;
            }
            else
            {
                double inv = 1.0 / ray.Direction.Z;
                double t1 = (Min.Z - ray.Origin.Z) * inv;
                double t2 = (Max.Z - ray.Origin.Z) * inv;
                if (t1 > t2) { var tmp = t1; t1 = t2; t2 = tmp; }
                if (t1 > tmin) tmin = t1;
                if (t2 < tmax) tmax = t2;
                if (tmin > tmax) return false;
            }

            return tmax >= 0;
        }

        public RayHit Intersect(Ray ray)
        {
            if (!IntersectAABB(ray)) return null;

            RayHit bestHit = null;
            double bestDistSq = double.PositiveInfinity;

            if (Triangles != null)
            {
                foreach (var tri in Triangles)
                {
                    RayHit hit = tri.Intersect(ray);
                    if (hit == null) continue;

                    Vector3 diff = hit.Position - ray.Origin;
                    double distSq = diff.LengthSq();

                    if (distSq > 0 && distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestHit = hit;
                    }
                }
                return bestHit;
            }
            else
            {
                RayHit leftHit = Left?.Intersect(ray);
                RayHit rightHit = Right?.Intersect(ray);

                if (leftHit == null) return rightHit;
                if (rightHit == null) return leftHit;

                double leftDistSq = (leftHit.Position - ray.Origin).LengthSq();
                double rightDistSq = (rightHit.Position - ray.Origin).LengthSq();
                return (leftDistSq < rightDistSq) ? leftHit : rightHit;
            }
        }
    }
}