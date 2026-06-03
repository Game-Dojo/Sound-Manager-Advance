using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using Audio;
using UnityEngine.Audio;

public class AudioEnumGenerator : AssetPostprocessor
{
    private const string EnumPath = "Assets/Scripts/Audio/AudioID.cs";
    private const string GroupEnumPath = "Assets/Scripts/Audio/AudioGroupID.cs";
    private const string AudioFolder = "Assets/Resources/Audio/";

    /*private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        GenerateEnum();
    }*/

    [MenuItem("Tools/Generate Audio Enums")]
    public static void GenerateEnum()
    {
        if (!Directory.Exists(AudioFolder))
        {
            Debug.LogWarning("Folder does not exist: " + AudioFolder);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder });
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
            UnityEditor.AssetDatabase.CreateAsset(newAsset, $"Assets/Scriptables/{name}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        File.WriteAllText(EnumPath, sb.ToString());
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Generate Group Enums")]
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
}