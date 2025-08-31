
using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material uses an image
    /// as a texture map for the color and optionally a normal map for surface
    /// details. It extends the base Material class to include texture properties.
    /// </summary>
    public class TextureMaterial : Material
    {
        private Image colorMap;
        private Image normalMap;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="colorMap">The color map (texture) of the material</param>
        /// <param name="normalMap">The normal map of the material (optional)</param>
        public TextureMaterial(Image colorMap, Image normalMap = null)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.colorMap = colorMap;
            this.normalMap = normalMap;
        }

        /// <summary>
        /// Get the color of the material at a given texture coordinate.
        /// </summary>
        /// <param name="uv">a two-dimensional texture coordinate</param>
        /// <returns>The color at the specified texture coordinate</returns>
        override
        public Color GetDiffuseColor(TextureCoord uv)
        {
            // Stage 3.2 - B1: Colour texture mapping
            // For basic materials, return the base color. Texture materials
            // will override this method to provide texture-specific colors.
            int x = (int)(uv.U * (colorMap.Width - 1));
            int y = (int)((1 - uv.V) * (colorMap.Height - 1));
            return colorMap.GetPixel(x, y);
        }
    }
}
