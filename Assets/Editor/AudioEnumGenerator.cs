using Audio;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Audio.AudioSettings;

namespace Editor
{
    public class AudioEnumGenerator : AssetPostprocessor
    {
        private const string GroupEnumFileName = "AudioGroupID.cs";
        private const string AudioEnumFileName = "AudioID.cs";

        #region Generation
        public static void GenerateEnum()
        {
            var audioPath = GetAudioResourcePath();
            var scriptablePath = GetScriptablePath();
            var scriptsPath = GetScriptPath();
            
            if (audioPath == "" || scriptablePath == "" || scriptsPath == "")
            {
                Debug.LogWarning("Resources/Scriptable or Script paths are invalid");
                return;
            }
            
            if (!Directory.Exists(audioPath))
            {
                Debug.LogWarning("Folder does not exist: " + audioPath);
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { audioPath });
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// AUTO-GENERATED CODE - DO NOT MODIFY");
            sb.AppendLine("namespace Audio");
            sb.AppendLine("{");
            sb.AppendLine("    public enum AudioID");
            sb.AppendLine("    {");
            sb.AppendLine("        None = 0,");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path).Replace(" ", "_");
                sb.AppendLine($"        {name},");

                var scrPath = $"{scriptablePath}{name}.asset";
                if (File.Exists(scrPath)) continue;
                
                AudioScriptable newAsset = ScriptableObject.CreateInstance<AudioScriptable>();
                UnityEditor.AssetDatabase.CreateAsset(newAsset, scrPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
        
            File.WriteAllText($"{scriptsPath}{AudioEnumFileName}", sb.ToString());
            AssetDatabase.Refresh();
        }

        public static void GenerateGroupsEnum()
        {
            var audioResourcesPath = GetAudioResourcePath();
            var scriptsPath = GetScriptPath();
            if (audioResourcesPath == "" || scriptsPath == "")
            {
                Debug.LogWarning("Resources & Scripts paths are invalid");
                return;
            }
            
            if (!Directory.Exists(audioResourcesPath)) return;

            var mixer = Resources.Load<AudioMixer>("MainMixer");
            if (!mixer)
            {
                Debug.LogWarning("AudioMixer not found in Resources!");
                return;
            }

            var allGroups = mixer.FindMatchingGroups(""); //new string[] {"Music", "SFX", "UI"};
        
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// AUTO-GENERATED CODE - DO NOT MODIFY");
            sb.AppendLine("namespace Audio");
            sb.AppendLine("{");
            sb.AppendLine("    public enum AudioGroupID");
            sb.AppendLine("    {");
            sb.AppendLine("        None = 0,");

            foreach (AudioMixerGroup group in allGroups)
            {
                var groupName = group.ToString();
                if (groupName != "Master")
                    sb.AppendLine($"        {groupName},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
        
            File.WriteAllText($"{scriptsPath}{GroupEnumFileName}", sb.ToString());
            AssetDatabase.Refresh();
        }

        private static void GenerateEmptyAudioList()
        {
            var audioResourcesPath = GetAudioResourcePath();
            var scriptsPath = GetScriptPath();
            if (audioResourcesPath == "" || scriptsPath == "")
            {
                Debug.LogWarning("Resources & Scripts paths are invalid");
                return;
            }
            
            if (!Directory.Exists(audioResourcesPath)) return;
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// AUTO-GENERATED CODE - DO NOT MODIFY");
            sb.AppendLine("namespace Audio");
            sb.AppendLine("{");
            sb.AppendLine("    public enum AudioID");
            sb.AppendLine("    {");
            sb.AppendLine("        None = 0");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        
            File.WriteAllText($"{scriptsPath}{AudioEnumFileName}", sb.ToString());
            AssetDatabase.Refresh();
        }

        private static void GenerateEmptyAudioGroups()
        {
            var audioResourcesPath = GetAudioResourcePath();
            var scriptsPath = GetScriptPath();
            if (audioResourcesPath == "" || scriptsPath == "")
            {
                Debug.LogWarning("Resources & Scripts paths are invalid");
                return;
            }
            
            if (!Directory.Exists(audioResourcesPath)) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// AUTO-GENERATED CODE - DO NOT MODIFY");
            sb.AppendLine("namespace Audio");
            sb.AppendLine("{");
            sb.AppendLine("    public enum AudioGroupID");
            sb.AppendLine("    {");
            sb.AppendLine("        None = 0");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        
            File.WriteAllText($"{scriptsPath}{GroupEnumFileName}", sb.ToString());
            AssetDatabase.Refresh();
        }
        #endregion
        
        #region Clean up
        public static void CleanAll()
        {
            GenerateEmptyAudioGroups();
            GenerateEmptyAudioList();
        }
        #endregion
        
        #region Settings
        private static AudioSettings GetAudioSetting()
        {
            AudioSettings mySo = AssetDatabase.LoadAssetAtPath<AudioSettings>(AudioSetupWindow.SettingsPath);
            return mySo;
        }

        private static string GetScriptPath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null) return null;
            return mySo.scriptsAudioPath;
        }
        
        public static string GetAudioResourcePath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null)
            {
                Debug.LogWarning("Audio settings not found");
                return null;
            }
            return mySo.audioResourcesPath;
        }
        
        public static string GetScriptablePath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null) return null;
            return mySo.scriptablesPath;
        }
        #endregion
    }
}