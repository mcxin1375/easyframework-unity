using System.Linq;
using System.Text;

namespace EasyFramework.Editor
{
    public static class SVNHelper
    {
        public static string Del(params string[] files)
        {
            if (files == null || files.Length == 0) return string.Empty;
            //StringBuilder sb = new StringBuilder();
            //foreach (var file in files) sb.Append($" \"{file}\"");
            //var filesStr = sb.ToString();
            //return $"delete{filesStr}";
            return $"delete {string.Join(" ", files.Select(f => $"\"{f}\""))}";
        }

        public static string Add(params string[] files)
        {
            if (files == null || files.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var file in files) sb.Append($" \"{file}\"");
            var filesStr = sb.ToString();
            return $"add --parents --force {filesStr}";
        }

        public static string Status(params string[] files)
        {
            if (files == null || files.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var file in files) sb.Append($" \"{file}\"");
            var filesStr = sb.ToString();
            return $"status {filesStr}";
        }

        public static string Commit(string file, string desc)
        {
            if (string.IsNullOrWhiteSpace(file)) return string.Empty;
            return $"commit -m \"{desc}\" \"{file}\"";
        }

        public static string Commit(string[] files, string desc)
        {
            if (files == null || files.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var file in files) sb.Append($" \"{file}\"");
            var filesStr = sb.ToString();
            return $"commit -m \"{desc}\" {filesStr}";
        }

        public static string CheckOut(string url, string path, int revision = 0)
        {
            if (revision > 0)
            {
                return $"checkout -r {revision} \"{url}\" \"{path}\"";
            }
            return $"checkout \"{url}\" \"{path}\"";
        }

        public static string Update(string path, int revision = 0)
        {
            if (revision > 0)
            {
                return $"update -r {revision} \"{path}\"";
            }
            return $"update \"{path}\"";
        }

        public static string Delete(string path) => $"rm \"{path}\"";
        public static string Cleanup(string path) => $"cleanup \"{path}\"";
        public static string Info(string path) => $"info \"{path}\"";
        public static string Revert(string path) => $"revert -R \"{path}\"";

    }
}
