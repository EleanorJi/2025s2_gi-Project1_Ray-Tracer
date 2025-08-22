using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private Camera camera;
        private Color ambientLightColor;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        private ISet<Animation> animations;
        private const double offset = 1e-10;
        private const int DefaultMaxRecursionDepth = 5;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.camera = new Camera(Transform.Identity);
            this.ambientLightColor = new Color(0, 0, 0);
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
            this.animations = new HashSet<Animation>();
        }

        /// <summary>
        /// Set the camera for the scene.
        /// </summary>
        /// <param name="camera">Camera object</param>
        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        /// <summary>
        /// Set the ambient light color for the scene.
        /// </summary>
        /// <param name="color">Color object</param>
        public void SetAmbientLightColor(Color color)
        {
            this.ambientLightColor = color;
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Add an animation to the scene.
        /// </summary>
        /// <param name="animation">Animation object</param>
        public void AddAnimation(Animation animation)
        {
            this.animations.Add(animation);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds</param>
        public void Render(Image outputImage, double time = 0)
        {
            // widths and heights of output image
            int width = outputImage.Width;
            int height = outputImage.Height;
            double aspectRatio = (double)width / height;

            // camera position
            Vector3 cameraPosition = new Vector3(0, 0, 0);

            // horizontal FOV and vertical FOV
            double horiFov = 60.0 * Math.PI / 180.0;
            double vertFov = 2 * Math.Atan(Math.Tan(horiFov / 2) / aspectRatio);

            // half-width and half-height of the imaging plane at z = 1
            double tanHalfHoriFov = Math.Tan(horiFov / 2);
            double tanHalfVertFov = Math.Tan(vertFov / 2);


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Stage 1.3 - Fire a ray for each pixel
                    double rayX = ((x + 0.5) / width * 2 - 1) * tanHalfHoriFov;
                    double rayY = (1 - (y + 0.5) / height * 2) * tanHalfVertFov;
                    Vector3 rayDirection = new Vector3(rayX, rayY, 1);
                    Ray ray = new Ray(cameraPosition, rayDirection);

                    // stage 2.3 - Reflection rays
                    Color pixelColor = Trace(ray, 0);

                    outputImage.SetPixel(x, y, pixelColor);
                }
            }
        }
        
        /// <summary>
        /// Recursively traces a ray through the scene and calculates color.
        /// </summary>
        /// <param name="ray">The ray to trace through the scene</param>
        /// <param name="depth">Current recursion depth</param>
        /// <returns>The computed color along the ray path</returns>
        private Color Trace(Ray ray, int depth)
        {

            // Find the nearest intersection point
            var (closestEntity, closestHit) = FindClosestHit(ray);

            // No intersection then return ambient light color
            if (closestHit == null)
                return ambientLightColor;

            // Calculate local illumination
            Color localColor = LocalIllumination(closestHit, closestEntity, ray.Origin);

            // If reaches the maximum recursion depth, directly return the local illumination.
            if (depth >= DefaultMaxRecursionDepth)
                return localColor;

            Vector3 N = closestHit.Normal;
            Vector3 D = ray.Direction.Normalized();

            Color reflectedColor = ambientLightColor;
            Color refractedColor = ambientLightColor;

            // Stage 2.3 - Reflection rays
            if (closestEntity.Material.Reflectivity > 0)
            {
                Vector3 reflectedDir = D - 2 * D.Dot(N) * N;
                Vector3 reflectedOrigin = closestHit.Position + offset * N;
                Ray reflectedRay = new Ray(reflectedOrigin, reflectedDir.Normalized());
                reflectedColor = Trace(reflectedRay, depth + 1);
            }

            // Stage 2.4 – Refraction rays
            if (closestEntity.Material.Transmissivity > 0)
            {
                double nI, nT;

                // Determine whether to enter or exit the object
                if (D.Dot(N) > 0)
                {
                    // exit, from the object to the air
                    nI = closestEntity.Material.RefractiveIndex;
                    nT = 1.0;
                    // flip the normal
                    N = -N;
                }
                else
                {
                    // enter, from the air entering the object
                    nI = 1.0;
                    nT = closestEntity.Material.RefractiveIndex;
                }

                double refractedRatio = nI / nT;
                double cosThetaI = -D.Dot(N);
                double sin2ThetaT = refractedRatio * refractedRatio * (1.0 - cosThetaI * cosThetaI);

                // No total internal reflection
                if (sin2ThetaT <= 1.0)
                {
                    double cosThetaT = Math.Sqrt(1.0 - sin2ThetaT);
                    Vector3 refractedDir = refractedRatio * D + (refractedRatio * cosThetaI - cosThetaT) * N;
                    Vector3 refractedOrigin = closestHit.Position - offset * N;
                    Ray refractedRay = new Ray(refractedOrigin, refractedDir.Normalized());

                    refractedColor = Trace(refractedRay, depth + 1);
                }
                // Otherwise: Total internal reflection, just ignore the refracted part
            }

            // combine color
            Color finalColor = localColor + reflectedColor * closestEntity.Material.Reflectivity + refractedColor * closestEntity.Material.Transmissivity;
            return finalColor;
        }


        /// <summary>
        /// Finds the closest intersection point between a ray and all entities in the scene.
        /// </summary>
        /// <param name="ray">The ray to test for intersections</param>
        /// <returns>the closest entity and the hit data</returns>
        private (SceneEntity entity, RayHit hit) FindClosestHit(Ray ray)
        {
            double closestT = double.PositiveInfinity;
            SceneEntity closestEntity = null;
            RayHit closestHit = null;
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit != null)
                {
                    double currentT = (hit.Position - ray.Origin).LengthSq();

                    // If object is closer, then update the color.
                    if (currentT > 0 && currentT < closestT)
                    {
                        closestT = currentT;
                        closestEntity = entity;
                        closestHit = hit;
                    }
                }
            }
            return (closestEntity, closestHit);
        }

        /// <summary>
        /// Computes the local illumination at a ray hit point.
        /// This combines ambient, diffuse, and specular lighting contributions from all light sources in the scene.
        /// </summary>
        /// <param name="hit">Ray hit data containing position, normal, and incident direction</param>
        /// <param name="entity">The scene entity that was hit</param>
        /// <param name="cameraPosition">Position of the camera in world space</param>
        /// <returns>The computed color at the hit point, accounting for all light sources</returns>
        private Color LocalIllumination(RayHit hit, SceneEntity entity, Vector3 cameraPosition)
        {
            // Ambient reflection
            Color ambient = entity.Material.AmbientColor * ambientLightColor;

            // initinal
            Color local = ambient;
            Vector3 vDir = (cameraPosition - hit.Position).Normalized();

            foreach (PointLight light in this.lights)
            {
                // Stage 2.2 - Shadow rays
                if (IsInShadow(hit, light))
                {
                    continue;
                }
                // Diffuse reflection
                Vector3 lDir = (light.Position - hit.Position).Normalized();
                double diffuseFactor = Math.Max(0, hit.Normal.Dot(lDir));
                Color diffuse = entity.Material.DiffuseColor * light.Color * diffuseFactor;
                local += diffuse;

                // Specular reflection
                Vector3 reflectDir = 2 * hit.Normal.Dot(lDir) * hit.Normal - lDir;
                double specularFactor = Math.Pow(Math.Max(0, reflectDir.Dot(vDir)), entity.Material.Shininess);
                Color specular = entity.Material.SpecularColor * light.Color * specularFactor;
                local += specular;
            }
            return local;
        }

        /// <summary>
        /// Checks if a point is in shadow with respect to a light source.
        /// </summary>
        /// <param name="hit">Ray hit data</param>
        /// <param name="light">The light source to check</param> 
        /// <returns>True if the point is in shadow (light is blocked by another object), false if light is visible</returns>
        private bool IsInShadow(RayHit hit, PointLight light)
        {
            Vector3 lDir = (light.Position - hit.Position).Normalized();
            double distanceToLight = (light.Position - hit.Position).Length();
            Vector3 shadowRayOrigin = hit.Position + offset * hit.Normal;
            Ray shadowRay = new Ray(shadowRayOrigin, lDir);

            foreach (SceneEntity entity in this.entities)
            {
                RayHit shadowHit = entity.Intersect(shadowRay);
                if (shadowHit != null)
                {
                    double distanceToHit = (shadowHit.Position - hit.Position).Length();
                    if (distanceToHit < distanceToLight)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
