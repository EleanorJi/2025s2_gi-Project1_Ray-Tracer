
using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material generates its
    /// texture procedurally based on defined patterns, such as stripes or
    /// checkers, rather than using an image file. 
    /// Stage 3.2 - B3: Procedural textures
    /// </summary>
    public class ProceduralMaterial : Material
    {
        public enum PatternType
        {
            Checkers,
            Stripes,
        }

        private PatternType pattern;
        private double scaleU;
        private double scaleV;
        private Color color1;
        private Color color2;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="pattern">The pattern of the material</param>
        /// <param name="scaleU">Scale factor in the U direction</param>
        /// <param name="scaleV">Scale factor in the V direction</param>
        /// <param name="color1">First color of the pattern</param>
        /// <param name="color2">Second color of the pattern</param>
        public ProceduralMaterial(PatternType pattern, double scaleU, double scaleV, Color color1, Color color2)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.pattern = pattern;
            this.scaleU = scaleU;
            this.scaleV = scaleV;
            this.color1 = color1;
            this.color2 = color2;
        }

        /// <summary>
        /// Get the color at the specified texture coordinates based on the procedural pattern.
        /// </summary>
        /// <param name="uv">a two-dimensional texture coordinate</param>
        /// <returns>The color at the specified texture coordinates</returns>
        public override Color GetDiffuseColor(TextureCoord uv)
        {
            // Apply scaling to texture coordinates
            double u = uv.U * scaleU;
            double v = uv.V * scaleV;

            switch (pattern)
            {
                case PatternType.Checkers:
                    return GetCheckersPattern(u, v);
                case PatternType.Stripes:
                    return GetStripesPattern(u, v);
                default:
                    return color1;
            }
        }

        /// <summary>
        /// Generate a checkers pattern.
        /// </summary>
        /// <param name="u">Scaled U coordinate</param>
        /// <param name="v">Scaled V coordinate</param>
        /// <returns>Color for the checkers pattern</returns>
        private Color GetCheckersPattern(double u, double v)
        {
            // checker index
            int uIndex = (int) u;
            int vIndex = (int) v;

            // Alternate colors
            if ((uIndex + vIndex) % 2 == 0)
            {
                return color1;
            }
            else
            {
                return color2;
            }
        }
        
        /// <summary>
        /// Generate a stripes pattern.
        /// </summary>
        /// <param name="u">Scaled U coordinate</param>
        /// <param name="v">Scaled V coordinate</param>
        /// <returns>Color for the stripes pattern</returns>
        private Color GetStripesPattern(double u, double v)
        {
            // stripe index
            int uIndex = (int) u;
            
            // Alternate colors
            if (uIndex % 2 == 0)
            {
                return color1;
            }
            else
            {
                return color2;
            }
        }
    }
}
