using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityToolbag
{
    public static class UnityConstantsGenerator
    {
        [MenuItem("Edit/Generate UnityConstants.cs")]
        public static void Generate()
        {
            // Try to find an existing file in the project called "UnityConstants.cs"
            string filePath = string.Empty;
            foreach (var file in Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)) {
                if (Path.GetFileNameWithoutExtension(file) == "UnityConstants") {
                    filePath = file;
                    break;
                }
            }

            // If no such file exists already, use the save panel to get a folder in which the file will be placed.
            if (string.IsNullOrEmpty(filePath)) {
                string directory = EditorUtility.OpenFolderPanel("Choose location for UnityConstants.cs", Application.dataPath, "");

                // Canceled choose? Do nothing.
                if (string.IsNullOrEmpty(directory)) {
                    return;
                }

                filePath = Path.Combine(directory, "UnityConstants.cs");
            }

            // Write out our file
            using (var writer = new StreamWriter(filePath)) {
                writer.WriteLine("// This file is auto-generated. Modifications are not saved.");
                writer.WriteLine();
                writer.WriteLine("namespace UnityConstants");
                writer.WriteLine("{");

                // Write out the tags
                writer.WriteLine("    public static class Tags");
                writer.WriteLine("    {");
                foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags) {
                    writer.WriteLine("        /// <summary>");
                    writer.WriteLine("        /// Name of tag '{0}'.", tag);
                    writer.WriteLine("        /// </summary>");
                    writer.WriteLine("        public const string {0} = \"{1}\";", MakeSafeForCode(tag), tag);
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Write out sorting layers
                var sortingLayerNames = SortingLayerHelper.sortingLayerNames;
                if (sortingLayerNames != null) {
                    writer.WriteLine("    public static class SortingLayers");
                    writer.WriteLine("    {");
                    for (int i = 0; i < sortingLayerNames.Length; i++) {
                        var name = sortingLayerNames[i];
                        int id = SortingLayerHelper.GetSortingLayerIDForName(name);
                        writer.WriteLine("        /// <summary>");
                        writer.WriteLine("        /// ID of sorting layer '{0}'.", name);
                        writer.WriteLine("        /// </summary>");
                        writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(name), id);
                    }
                    writer.WriteLine("    }");
                    writer.WriteLine();
                }

                // Write out layers
                writer.WriteLine("    public static class Layers");
                writer.WriteLine("    {");
                for (int i = 0; i < 32; i++) {
                    string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
                    if (!string.IsNullOrEmpty(layer)) {
                        writer.WriteLine("        /// <summary>");
                        writer.WriteLine("        /// Index of layer '{0}'.", layer);
                        writer.WriteLine("        /// </summary>");
                        writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(layer), i);
                    }
                }
                writer.WriteLine();
                for (int i = 0; i < 32; i++) {
                    string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
                    if (!string.IsNullOrEmpty(layer)) {
                        writer.WriteLine("        /// <summary>");
                        writer.WriteLine("        /// Bitmask of layer '{0}'.", layer);
                        writer.WriteLine("        /// </summary>");
                        writer.WriteLine("        public const int {0}Mask = 1 << {1};", MakeSafeForCode(layer), i);
                    }
                }
                writer.WriteLine("    }");
                writer.WriteLine();

                // Write out scenes
                writer.WriteLine("    public static class Scenes");
                writer.WriteLine("    {");
                writer.WriteLine("        /// <summary>");
                writer.WriteLine("        /// List of included scene names.");
                writer.WriteLine("        /// </summary>");
                writer.WriteLine("        public static readonly string[] SceneNames = new string[] { " + EditorBuildSettings.scenes.Aggregate(new StringBuilder(), (a, b) => {
                    if (a.Length > 0)
                        a.Append(", ");
                    a.AppendFormat("\"{0}\"", Path.GetFileNameWithoutExtension(b.path));
                    return a;
                }) + " };");

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    string scene = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
                    writer.WriteLine("        /// <summary>");
                    writer.WriteLine("        /// Name of '{0}'.", scene);
                    writer.WriteLine("        /// </summary>");
                    writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(scene), i);
                }

                writer.WriteLine();
                writer.WriteLine("\t\tpublic enum ScenesEnum");
                writer.WriteLine("\t\t{");

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    string scene = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
                    if(i == EditorBuildSettings.scenes.Length - 1)
                        writer.WriteLine(string.Format("\t\t\t{0} = {1}", MakeSafeForCode(scene), i));
                    else
                        writer.WriteLine(string.Format("\t\t\t{0} = {1},", MakeSafeForCode(scene), i));

                }
                writer.WriteLine("\t\t}");

                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.WriteLine();
            }

            // Refresh
            AssetDatabase.Refresh();
        }

        private static string MakeSafeForCode(string str)
        {
            str = Regex.Replace(str, "[^a-zA-Z0-9_]", "_", RegexOptions.Compiled);
            if (char.IsDigit(str[0])) {
                str = "_" + str;
            }
            return str;
        }
    }
}
