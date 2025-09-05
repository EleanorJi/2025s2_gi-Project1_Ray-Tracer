using System;
using System.Numerics;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Plane : SceneEntity
    {
        private Vector3 center;
        private Vector3 normal;
        private Material material;
        private const double offset = 1e-6;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        public Plane(Vector3 center, Vector3 normal, Material material)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the plane, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            double nr = ray.Direction.Dot(normal);
            // parallel to the plane
            if (Math.Abs(nr) < offset)
            {
                return null;
            }
            double t = - (ray.Origin - center).Dot(normal) / nr;
            Vector3 hitPoint = ray.Origin + t * ray.Direction;
            // opposite side of origin
            if (t < offset)
            {
                return null;
            }

            // Establish a local coordinate system
            Vector3 tangent = normal.Cross(new Vector3(0, 1, 0));
            if (tangent.LengthSq() < 1e-6)
            {
                tangent = normal.Cross(new Vector3(1, 0, 0));
            }
            tangent = tangent.Normalized();
            Vector3 bitangent = normal.Cross(tangent).Normalized();

            // Project the intersection point onto the plane, and obtain u and v.
            double u = (hitPoint - center).Dot(tangent);
            double v = (hitPoint - center).Dot(bitangent);

            double uWrapped = u - Math.Floor(u);
            double vWrapped = v - Math.Floor(v);
            TextureCoord texCoord = new TextureCoord((float)uWrapped, (float)vWrapped);

            return new RayHit(hitPoint, normal, ray.Direction, Material, texCoord);
        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
