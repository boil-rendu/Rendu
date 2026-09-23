using UnityEngine;

namespace RD.Init.Editor
{
    public class EditorHelper
    {
        private static string _projFolder;

        public static string GetProjPath(string relativePath = "")
        {
            if (string.IsNullOrEmpty(_projFolder))
            {
                var projFolder = Application.dataPath;
                _projFolder = projFolder.Substring(0, projFolder.Length - 7);
            }

            if (relativePath == null)
            {
                relativePath = "";
            }

            relativePath = relativePath.Trim();
            if (!string.IsNullOrEmpty(relativePath))
            {
                if (relativePath.Contains("\\"))
                {
                    relativePath = relativePath.Replace("\\", "/");
                }

                if (!relativePath.StartsWith("/"))
                {
                    relativePath = $"/{relativePath}";
                }
            }

            return $"{_projFolder}{relativePath}";
        }
    }
}
