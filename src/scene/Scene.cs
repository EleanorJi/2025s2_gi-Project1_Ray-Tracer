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
            for (int y = 0; y < outputImage.Height; y++)
            {
                for (int x = 0; x < outputImage.Width; x++)
                {
                    Color pixelColor = new Color(1, 1, 1);
                    outputImage.SetPixel(x, y, pixelColor);
                }
            }
        }
    }
}
