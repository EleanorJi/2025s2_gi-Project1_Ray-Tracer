using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 edge1 = this.v1 - this.v0;
            Vector3 edge2 = this.v2 - this.v0;
            Vector3 normal = edge1.Cross(edge2);
            double areaABC = normal.Length() / 2.0;

            // Check if the light is parallel to the plane of the triangle
            double nr = normal.Dot(ray.Direction);

            // parallel to the plane
            if (nr == 0)
            {
                return null;
            }

            // solve the t
            Vector3 ao = ray.Origin - this.v0;
            double t = -normal.Dot(ao) / nr;

            // pposite side of origin
            if (t < 0)
            {
                return null;
            }

            // Intersection point
            Vector3 hitPoint = ray.Origin + t * ray.Direction;

            // Check if P lies inside the Triangle
            Vector3 bp = hitPoint - this.v1;
            Vector3 cp = hitPoint - this.v2;
            double areaBCP = (bp.Cross(cp)).Length() / 2.0;
            double areaCAP = (edge2.Cross(-ao - t * ray.Direction)).Length() / 2.0;
            double areaABP = ((-ao - t * ray.Direction).Cross(edge1)).Length() / 2.0;
            double u = areaBCP / areaABC;
            double v = areaCAP / areaABC;
            double w = areaABP / areaABC;

            // P outside the triangle
            if (u < -1e-6 || v < -1e-6 || w < -1e-6 || Math.Abs(u + v + w - 1) > 1e-6)
            {
                return null;
            }

            // P inside the triangle, return the hit data
            return new RayHit(hitPoint, normal.Normalized(), ray.Direction, this.Material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
