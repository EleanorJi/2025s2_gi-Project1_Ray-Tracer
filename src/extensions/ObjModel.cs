using System.IO;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private string objFilePath;
        private Transform transform;
        private Material material;
        private List<Vector3> vertices;
        private List<Vector3> normals;
        private List<Triangle> triangles;
        private BVHNode root;

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Transform transform, Material material)
        {
            this.objFilePath = objFilePath;
            this.transform = transform;
            this.material = material;
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.triangles = new List<Triangle>();

            // Here's some code to get you started reading the file...
            string[] lines = File.ReadAllLines(objFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                // The current line is lines[i]
                string[] tokens = lines[i].Split(' ');

                if (tokens.Length == 0 || tokens.Length != 4)
                    continue;

                string start = tokens[0];
                if (start == "v")
                {
                    double x = double.Parse(tokens[1]);
                    double y = double.Parse(tokens[2]);
                    double z = double.Parse(tokens[3]);
                    Vector3 vertex = new Vector3(x, y, z);
                    vertex = transform.Apply(vertex);
                    vertices.Add(vertex);
                }
                else if (start == "vn")
                {
                    double nx = double.Parse(tokens[1]);
                    double ny = double.Parse(tokens[2]);
                    double nz = double.Parse(tokens[3]);
                    Vector3 normal = new Vector3(nx, ny, nz);
                    normal = transform.Rotation.Rotate(normal).Normalized();
                    normals.Add(normal);
                }
                else if (start == "f")
                {    
                    int idx0 = int.Parse(tokens[1].Split('/')[0]) - 1;
                    int idx1 = int.Parse(tokens[2].Split('/')[0]) - 1;
                    int idx2 = int.Parse(tokens[3].Split('/')[0]) - 1;
                    triangles.Add(new Triangle(vertices[idx0], vertices[idx1], vertices[idx2], material));
                }
            }

            // build BVH
            if (triangles.Count > 0)
            {
                root = new BVHNode(triangles);
            }
            else
            {
                root = null;
            }
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            if (root == null) return null;
            return root.Intersect(ray);
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
