using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace RD.Recast
{
    /// <summary>
    /// 网格数据导出器（负责场景网格收集、合并、导出）
    /// </summary>
    public static class MeshDataExporter
    {
        /// <summary>
        /// 从场景根节点收集所有 MeshFilter
        /// </summary>
        public static List<MeshFilter> CollectMeshFilters(Transform parent)
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            CollectMeshFiltersRecursive(parent, meshFilters);
            return meshFilters;
        }

        private static void CollectMeshFiltersRecursive(Transform parent, List<MeshFilter> result)
        {
            MeshFilter mf = parent.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                result.Add(mf);

            foreach (Transform child in parent)
            {
                CollectMeshFiltersRecursive(child, result);
            }
        }

        /// <summary>
        /// 合并多个 MeshFilter 为一个 Mesh
        /// </summary>
        public static Mesh CombineMeshes(List<MeshFilter> meshFilters)
        {
            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Count];
            for (int i = 0; i < meshFilters.Count; i++)
            {
                MeshFilter mf = meshFilters[i];
                combineInstances[i].mesh = mf.sharedMesh;
                combineInstances[i].transform = mf.transform.localToWorldMatrix;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances, true);
            return combinedMesh;
        }

        /// <summary>
        /// 导出网格数据到文件（vertices/triangles/obj）
        /// </summary>
        public static void ExportMeshData(Mesh mesh, string exportPath, string fileName)
        {
            // 转换顶点数据（X 轴取负）
            float[] vertices = ConvertVertices(mesh.vertices);
            int[] triangles = mesh.triangles;

            // 导出各个文件
            ExportVertices(vertices, exportPath, fileName);
            ExportTriangles(triangles, exportPath, fileName);
            ExportObj(mesh, exportPath, fileName);
        }

        private static float[] ConvertVertices(Vector3[] vertices)
        {
            float[] result = new float[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                result[i * 3] = -vertices[i].x; // X 轴取负修正镜像
                result[i * 3 + 1] = vertices[i].y;
                result[i * 3 + 2] = vertices[i].z;
            }
            return result;
        }

        private static void ExportVertices(float[] vertices, string exportPath, string fileName)
        {
            string vertexPath = Path.Combine(exportPath, $"{fileName}_vertices.txt");
            using (StreamWriter sw = new StreamWriter(vertexPath))
            {
                for (int i = 0; i < vertices.Length; i += 3)
                    sw.WriteLine($"{vertices[i]} {vertices[i + 1]} {vertices[i + 2]}");
            }
        }

        private static void ExportTriangles(int[] triangles, string exportPath, string fileName)
        {
            string trianglePath = Path.Combine(exportPath, $"{fileName}_triangles.txt");
            using (StreamWriter sw = new StreamWriter(trianglePath))
            {
                for (int i = 0; i < triangles.Length; i += 3)
                    sw.WriteLine($"{triangles[i + 2]} {triangles[i + 1]} {triangles[i]}"); // 倒序
            }
        }

        private static void ExportObj(Mesh mesh, string exportPath, string fileName)
        {
            string objPath = Path.Combine(exportPath, $"{fileName}.obj");
            using (StreamWriter sw = new StreamWriter(objPath))
            {
                // 顶点 X 轴取负
                foreach (Vector3 v in mesh.vertices)
                    sw.WriteLine($"v {-v.x} {v.y} {v.z}");

                // 法线同步 X 轴取负
                foreach (Vector3 n in mesh.normals)
                    sw.WriteLine($"vn {-n.x} {n.y} {n.z}");

                // 三角面顶点倒序，修正绕序
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                    sw.WriteLine($"f {mesh.triangles[i + 2] + 1} {mesh.triangles[i + 1] + 1} {mesh.triangles[i] + 1}");
            }
        }

        /// <summary>
        /// 从 TXT 文件读取网格数据
        /// </summary>
        public static (float[] vertices, int[] triangles) ReadMeshData(string path, string name)
        {
            string vertexPath = Path.Combine(path, $"{name}_vertices.txt");
            string trianglePath = Path.Combine(path, $"{name}_triangles.txt");

            if (!File.Exists(vertexPath) || !File.Exists(trianglePath))
            {
                UnityEngine.Debug.LogError($"找不到网格文件：\n{vertexPath}\n{trianglePath}");
                return (null, null);
            }

            // 读取顶点
            List<float> verts = new List<float>();
            foreach (var line in File.ReadAllLines(vertexPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ');
                verts.AddRange(new[] { float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]) });
            }

            // 读取三角面
            List<int> tris = new List<int>();
            foreach (var line in File.ReadAllLines(trianglePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ');
                tris.AddRange(new[] { int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]) });
            }

            return (verts.ToArray(), tris.ToArray());
        }
    }
}
