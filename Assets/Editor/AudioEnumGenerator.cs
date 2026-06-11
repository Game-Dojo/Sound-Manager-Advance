using System;
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
        private const string GroupEnumPath = "Assets/Scripts/Audio/AudioGroupID.cs";
        private const string EnumPath = "Assets/Scripts/Audio/AudioID.cs";
        
        private const string AudioFolder = "Assets/Resources/Audio/";

        public static void GenerateEnum()
        {
            var audioPath = GetAudioResourcePath();
            var scriptablePath = GetScriptablePath();

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
            
                AudioScriptable newAsset = ScriptableObject.CreateInstance<AudioScriptable>();
                UnityEditor.AssetDatabase.CreateAsset(newAsset, $"{scriptablePath}{name}.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
        
            File.WriteAllText(EnumPath, sb.ToString());
            AssetDatabase.Refresh();
        }

        public static void GenerateGroupsEnum()
        {
            if (!Directory.Exists(AudioFolder)) return;

            var mixer = Resources.Load<AudioMixer>("MainMixer");
            if (mixer == null) return;

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
        
            File.WriteAllText(GroupEnumPath, sb.ToString());
            AssetDatabase.Refresh();
        }

        public static void CleanAll()
        {
            if (IsGroupEnumCreated()) File.Delete(GroupEnumPath);
            if (IsAudioEnumCreated()) File.Delete(EnumPath);
        }
        
        public static bool IsAudioEnumCreated() => File.Exists(EnumPath);
        public static bool IsGroupEnumCreated() => File.Exists(GroupEnumPath);
        
        #region Settings
        private static AudioSettings GetAudioSetting()
        {
            AudioSettings mySo = AssetDatabase.LoadAssetAtPath<AudioSettings>("Assets/AudioSettings.asset");
            return mySo;
        }

        private static string GetScriptPath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null) return null;
            return mySo.scriptsAudioPath;
        }
        
        private static string GetAudioResourcePath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null)
            {
                Debug.LogWarning("Audio settings not found");
                return null;
            }
            return mySo.audioResourcesPath;
        }
        
        private static string GetScriptablePath()
        {
            AudioSettings mySo = GetAudioSetting();
            if (mySo == null) return null;
            return mySo.scriptablesPath;
        }
        #endregion
    }
}