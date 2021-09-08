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
        const string editorCommandKey = "customeditor_editorcommand";
        const string editorArgsKey = "customeditor_editorargs";
        const string codeAssetsKey = "customeditor_codeassets";
        const string terminalCommandKey = "customeditor_terminalcommand";
        const string terminalArgsKey = "customeditor_terminalargs";
        const string generateSlnKey = "customeditor_generatesln";

        static CustomExternalEditor()
        {
            var editor = new CustomExternalEditor();
            CodeEditor.Register(editor);
        }

        public CodeEditor.Installation[] Installations
        {
            get {
                return new CodeEditor.Installation[] {
                    new CodeEditor.Installation {
                        Name = "Custom",
                        Path = "/bin/false",
                    },
                };
            }
        }

        public void Initialize(string editorInstallationPath)
        {
            EditorPrefs.SetString(editorCommandKey, editorInstallationPath);
        }

        static string GetEditorCommand()
        {
            return EditorPrefs.GetString(editorCommandKey, "/bin/false");
        }

        static string GetEditorArgs()
        {
            return EditorPrefs.GetString(editorArgsKey, "");
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

        static string GetTerminalCommand()
        {
            return EditorPrefs.GetString(terminalCommandKey, "");
        }

        static string GetTerminalArgs()
        {
            return EditorPrefs.GetString(terminalArgsKey, "");
        }

        static bool GetGenerateSln()
        {
            return EditorPrefs.GetBool(generateSlnKey, true);
        }

        public void OnGUI()
        {
            var style = new GUIStyle {
                richText = true,
                margin = new RectOffset(0, 4, 0, 0)
            };

            using (new EditorGUI.IndentLevelScope())
            {
                string prevEditorCommand = GetEditorCommand();
                string newEditorCommand = EditorGUILayout.TextField(
                    new GUIContent(
                        "Custom editor command",
                        "Command to run custom editor."),
                        prevEditorCommand
                    );
                newEditorCommand = newEditorCommand.Trim();
                if (newEditorCommand != prevEditorCommand) {
                    EditorPrefs.SetString(editorCommandKey, newEditorCommand);
                }
                if (string.IsNullOrEmpty(newEditorCommand)) {
                    EditorGUILayout.HelpBox(
                        "No custom editor command is set.",
                        MessageType.Error
                    );
                }

                string prevEditorArgs = GetEditorArgs();
                string newEditorArgs = EditorGUILayout.TextField(
                    new GUIContent(
                        "Custom editor arguments",
                        "Arguments to call editor with.\n" +
                        "You can use folowing templates:\n" +
                        "%f - file to edit\n" +
                        "%l - line in file\n" +
                        "%c - column in file\n" +
                        "%p - path to current project Assets folder"),
                        prevEditorArgs
                    );
                newEditorArgs = newEditorArgs.Trim();
                if (newEditorArgs != prevEditorArgs) {
                    EditorPrefs.SetString(editorArgsKey, newEditorArgs);
                }

                string prevCodeAssets = GetCodeAssets();
                string newCodeAssets = EditorGUILayout.TextField(
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
                if (GUILayout.Button(
                        "Reset file extensions",
                        GUILayout.Width(200))
                    )
                {
                    EditorPrefs.DeleteKey(codeAssetsKey);
                }

                string prevTerminalCommand = GetTerminalCommand();
                string newTerminalCommand = EditorGUILayout.TextField(
                    new GUIContent(
                        "Custom terminal command",
                        "If you want to run your editor in terminal, specify terminal command here. If empty, editor will be ran on its own."),
                        prevTerminalCommand
                    );
                newTerminalCommand = newTerminalCommand.Trim();
                if (newTerminalCommand != prevTerminalCommand) {
                    EditorPrefs.SetString(terminalCommandKey, newTerminalCommand);
                }

                string prevTerminalArgs = GetTerminalArgs();
                string newTerminalArgs = EditorGUILayout.TextField(
                    new GUIContent(
                        "Terminal arguments",
                        "Arguments to call terminal with.\n" +
                        "You can use folowing templates:\n" +
                        "%e - editor command\n" +
                        "%a - editor arguments\n"),
                        prevTerminalArgs
                    );
                newTerminalArgs = newTerminalArgs.Trim();
                if (newTerminalArgs != prevTerminalArgs) {
                    EditorPrefs.SetString(terminalArgsKey, newTerminalArgs);
                }

                bool prevGenerateSln = GetGenerateSln();
                bool newGenerateSln = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Generate Visual Studio Solution",
                        "Generate sln and csproj when user clicks 'Open C# Project'. Useful for debugging with Visual Studio, working with vscode, using OmniSharp, etc."),
                        prevGenerateSln
                );
                if (newGenerateSln != prevGenerateSln) {
                    EditorPrefs.SetBool(generateSlnKey, newGenerateSln);
                }
            }
        }

        public bool OpenProject(string filePath, int line, int column)
        {
            if (!IsCodeAsset(filePath)) {
                return false;
            }

            ProcessStartInfo process = BuildEditor(filePath, line, column);
            if (process == null) {
                Debug.LogError($"[CustomExternalEditor] Failed to create editor command");
                return false;
            }

            if (!string.IsNullOrEmpty(GetTerminalCommand())) {
                process = BuildTerminal(process);
                if (process == null) {
                    Debug.LogError($"[CustomExternalEditor] Failed to create terminal command");
                    return false;
                }
            }

            Process.Start(process);

            return true;
        }

        public void SyncAll()
        {
            if (GetGenerateSln()) {
                RegenerateSln();
            }
        }

        public void SyncIfNeeded(
            string[] addedFiles, string[] deletedFiles,
            string[] movedFiles, string[] movedFromFiles,
            string[] importedFiles)
        {
            if (!GetGenerateSln()) {
                return;
            }

            bool regenerate = false;
            List<string> allFiles = new List<string>();
            allFiles.AddRange(addedFiles);
            allFiles.AddRange(deletedFiles);
            allFiles.AddRange(movedFiles);
            allFiles.AddRange(movedFromFiles);
            allFiles.AddRange(importedFiles);

            foreach (string file in allFiles) {
                if (IsCodeAsset(file)) {
                    regenerate = true;
                    break;
                }
            }

            if (regenerate) {
                RegenerateSln();
            }
        }

        public bool TryGetInstallationForPath(
            string editorPath,
            out CodeEditor.Installation installation)
        {
            installation = Installations.FirstOrDefault(
                install => install.Path == editorPath
            );
            return !string.IsNullOrEmpty(installation.Name);
        }

        static bool IsCodeAsset(string filePath)
        {
            string[] extensions = GetCodeAssetsAsList();
            string match = extensions.FirstOrDefault(ext => filePath.EndsWith(ext));
            return match != null;
        }

        static void RegenerateSln()
        {
			var syncVsType = Type.GetType("UnityEditor.SyncVS,UnityEditor");
			var synchronizerField = syncVsType.GetField(
                "Synchronizer",
                BindingFlags.NonPublic | BindingFlags.Static
            );
			var synchronizerObject = synchronizerField.GetValue(syncVsType);
			var synchronizerType = synchronizerObject.GetType();
			var synchronizerSyncFn = synchronizerType.GetMethod(
                "Sync",
                BindingFlags.Public | BindingFlags.Instance
            );

			synchronizerSyncFn.Invoke(synchronizerObject, null);

            Debug.Log($"[CustomExternalEditor] Regenerated Visual Studio solution");
        }

        static string GetFilePathInFolder(string folder, string file)
        {
            if (!string.IsNullOrEmpty(folder)) {
                string path = Path.Combine(folder, file);
                if (File.Exists(path)) {
                    return path;
                }
            }

            return null;
        }

        static string GetSystemFilePath(string file)
        {
            string[] pathDirs = Environment.GetEnvironmentVariable("PATH")
                .Split(Path.PathSeparator);

            foreach (string path in pathDirs) {
                string editorPath = GetFilePathInFolder(path, file);
                if (!string.IsNullOrEmpty(editorPath))
                    return editorPath;
            }

            return null;
        }

        static ProcessStartInfo BuildEditor(string filePath, int line, int column)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = GetSystemFilePath(GetEditorCommand());
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = GetEditorArgs()
                .Replace("%f", filePath)
                .Replace("%l", Math.Max(line, 0).ToString())
                .Replace("%c", Math.Max(column, 0).ToString())
                .Replace("%p", Application.dataPath);

			if (string.IsNullOrEmpty(startInfo.FileName)) {
                return null;
            }

            return startInfo;
        }

        static ProcessStartInfo BuildTerminal(ProcessStartInfo editor)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = GetSystemFilePath(GetTerminalCommand());
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = GetTerminalArgs()
                .Replace("%e", editor.FileName)
                .Replace("%a", editor.Arguments);

			if (string.IsNullOrEmpty(startInfo.FileName)) {
                return null;
            }

            return startInfo;
        }
    }
}
