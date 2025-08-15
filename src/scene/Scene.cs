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
            double aspectRatio = (double) width/height;

            // camera position
            Vector3 camera = new Vector3(0, 0, 0);

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
                    Ray ray = new Ray(camera, rayDirection);

                    Color pixelColor = new Color(0, 0, 0);
                    
                    // Stage 1.5 - Output primitives as solid colours
                    double closestT = double.PositiveInfinity;
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
                                pixelColor = entity.Material.DiffuseColor;
                            }
                        }
                    }
                    outputImage.SetPixel(x, y, pixelColor);
                }
            }
        }
    }
}
