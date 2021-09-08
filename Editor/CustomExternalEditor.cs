using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Custom.Editor
{
    [InitializeOnLoad]
    public class CustomExternalEditor : IExternalCodeEditor
    {
        const string pathKey = "customeditor_path";
        const string codeAssetsKey = "customeditor_codeassets";

        static CustomExternalEditor()
        {
            CodeEditor.Register(this)
        }

        public CodeEditor.Installations[] Installations
        {
            var installations = new List<CodeEditor.Installation>() {
                Name = "Custom editor",
                Path = GetPath(),
            };

            return installations;
        }

        public void Initialize(string editorInstallationPath)
        {
            EditorPrefs.SetString(pathKey, editorInstallationPath);
        }

        static string GetPath()
        {
            return EditorPrefs.GetString(pathKey, "");
        }

        static string GetCodeAssets()
        {
            return EditorPrefs.GetString(
                codeAssetsKey,
                ".cs,.shader,.h,.m,.c,.cpp,.txt,.md,.json"
            );
        }

        static string[] GetCodeAssetsAsList()
        {
            return GetCodeAssets().Split(',');
        }

        public void OnGUI()
        {
            var style = new GUIStyle {
                richText = true,
                margin = new RectOffset(0, 4, 0, 0)
            };

            using (new EditorGUI.IndentLevelScope())
            {
                var prevPath = GetPath();
                var newPath = EditorGUILayout.TextField(
                    new GUIContent(
                        "Custom editor path",
                        "Path to custom editor binary."),
                        prevPath
                    );
                newPath = newPath.Trim();
                if (newPath != prevPath) {
                    EditorPrefs.SetString(pathKey, newPath);
                }
                if (string.IsNullOrEmpty(newCodeAssets)) {
                    EditorGUILayout.HelpBox("No custom editor path is set.", MessageType.Error);
                }

                var prevCodeAssets = GetCodeAssets();
                var newCodeAssets = EditorGUILayout.TextField(
                    new GUIContent(
                        "File extensions",
                        "Comma-separated list of file extensions to open in custom editor. Clear it to open all files in custom editor."),
                        prevCodeAssets
                    );
                newCodeAssets = newCodeAssets.Trim();
                if (newCodeAssets != prevCodeAssets) {
                    EditorPrefs.SetString(codeAssetsKey, newCodeAssets);
                }
                if (string.IsNullOrEmpty(newCodeAssets)) {
                    EditorGUILayout.HelpBox(
                        "All files will be opened in custom editor.",
                        MessageType.Info
                    );
                }
                if (GUILayout.Button("Reset file extensions", GUILayout.Width(200))) {
                    EditorPrefs.DeleteKey(codeAssetsKey);
                }
            }
        }

        public bool OpenProject(string filePath, int line, int column)
        {
            if (!IsCodeAsset(filePath)) {
                return false;
            }

            //@TODO OpenProject

            return true;
        }

        public void SyncAll()
        {
            /*@TODO
            if (ShouldGenerateVisualStudioSln())
            {
                RegenerateVisualStudioSolution();
            }
            */
        }

        public void SyncIfNeeded(
            string[] addedFiles, string[] deletedFiles,
            string[] movedFiles, string[] movedFromFiles,
            string[] importedFiles)
        {
            //@TODO
        }

        public bool TryGetInstallationForPath(
            string editorPath,
            out CodeEditor.Installation installation)
        {
            installation = Installations.FirstOrDefault(install => install.Path == editorPath);
            return !string.IsNullOrEmpty(installation.Name);
        }
    }
}
