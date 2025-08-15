using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // basic caculate
            Vector3 oc = ray.Origin - this.center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2 * oc.Dot(ray.Direction);
            double c = oc.Dot(oc) - this.radius * this.radius;
            double delta = b * b - 4 * a * c;

            // no intersection
            if (delta < 0)
            {
                return null;
            }
            // one intersection
            else if (delta == 0)
            {
                double t = -b / (2 * a);
                if (t > 0)
                {
                    return CreateHit(ray, t);
                }
                else
                {
                    return null;
                }
            }
            // two intersections
            else
            {
                double sqrtDelta = Math.Sqrt(delta);
                double t1 = (-b - sqrtDelta) / (2 * a);
                double t2 = (-b + sqrtDelta) / (2 * a);

                // entry intersection
                if (t1 > 0)
                {
                    return CreateHit(ray, t1);
                }
                // exit intersection
                else if (t2 > 0)
                {
                    return CreateHit(ray, t2);
                }
                // both intersection points are on the opposite side of the light ray
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Create the hit of the sphere.
        /// </summary>
        /// <param name="ray">The ray to the sphere</param>
        /// <param name="t">t of the ray</param>
        /// <returns>Hit data </returns>
        private RayHit CreateHit(Ray ray, double t)
        {
            Vector3 hitPoint = ray.Origin + t * ray.Direction;
            Vector3 normal = (hitPoint - this.center).Normalized();
            return new RayHit(hitPoint, normal, ray.Direction, this.Material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
