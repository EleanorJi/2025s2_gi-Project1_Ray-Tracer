using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private TextureCoord uv0, uv1, uv2;
        private Material material;
        private const double offset = 1e-10;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        /// <param name="uv0">Texture coordinate for the first vertex</param>
        /// <param name="uv1">Texture coordinate for the second vertex</param>
        /// <param name="uv2">Texture coordinate for the third vertex</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material,
                TextureCoord uv0 = default, TextureCoord uv1 = default, TextureCoord uv2 = default)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
            this.uv0 = uv0;
            this.uv1 = uv1;
            this.uv2 = uv2;
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
            if (Math.Abs(nr) < offset)
            {
                return null;
            }

            // solve the t
            Vector3 ao = ray.Origin - this.v0;
            double t = -normal.Dot(ao) / nr;

            // pposite side of origin
            if (t < offset)
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
            if (u < -offset || v < -offset || w < -offset || Math.Abs(u + v + w - 1) > offset)
            {
                return null;
            }

            // P inside the triangle, return the hit data
            TextureCoord uv = u * uv0 + v * uv1 + w * uv2;
            return new RayHit(hitPoint, normal.Normalized(), ray.Direction, this.Material, uv);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
        
        public Vector3 V0 { get { return v0; } }
        public Vector3 V1 { get { return v1; } }
        public Vector3 V2 { get { return v2; } }
    }
}
