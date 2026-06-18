using Audio;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Audio.AudioSettings;

#if UNITY_EDITOR
namespace Editor
{
    public class AudioSetupWindow : EditorWindow
    {
        private int _toolbarIndex = 0;
        private readonly string[] _toolbarLabels = { "Setup", "Preferences" };

        private GUIStyle _headerStyle;
        private GUIStyle _greenButtonStyle;
        private GUIStyle _yellowButtonStyle;
        private GUIStyle _footerStyle;
        private GUIStyle _miniLabelStyle;
        
        [SerializeField] private static AudioMixer _targetMixer;
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public const string SettingsPath = "Assets/AudioSettings.asset";
        private const string AudioPath = "Audio";
        
        private string _resourcesPath = "Resources";
        private string _scriptablePath = "Scriptables";
        private string _scriptsPath = "Scripts";
        
        [MenuItem("Tools/Audio Manager")]
        public static void ShowWindow()
        {
            AudioSetupWindow window = GetWindow<AudioSetupWindow>("Audio Manager Panel");
            window.minSize = new Vector2(300, 500);

            FindAudioSettings();
        }

        private void OnGUI()
        {
            InitStyles();
            _toolbarIndex = GUILayout.Toolbar(_toolbarIndex, _toolbarLabels, GUILayout.Height(25));

            EditorGUILayout.BeginVertical();

            switch (_toolbarIndex)
            {
                case 0:
                    DrawSetupTab();
                    break;
                case 1:
                    DrawPreferencesTab();
                    break;
            }

            EditorGUILayout.EndVertical();

            // Footer Area
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("developed by Game Dojo", _footerStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        #region Tabs Layout
        private void DrawSetupTab()
        {
            Rect bannerRect = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            GUI.Box(bannerRect, "AUDIO MANAGER", _headerStyle);

            GUILayout.Space(10);

            int audioIDCount = System.Enum.GetNames(typeof(AudioID)).Length;
            int audioGroupCount = System.Enum.GetNames(typeof(AudioGroupID)).Length;
            
            EditorGUILayout.LabelField("AudioManager v0.0.1 [Release build]", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Group Enums: " + ((audioGroupCount > 1) ? "<color=green>Created!</color>" : "<color=red>not created</color>"), _miniLabelStyle);
            EditorGUILayout.LabelField("Audio Enums: " + ((audioIDCount > 1) ? "<color=green>Created!</color>" : "<color=red>not created</color>"), _miniLabelStyle);
            
            GUILayout.Space(15);

            Color defaultColor = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.1f, 0.4f, 0.1f); // Dark Green
            
            if (GUILayout.Button("1. Create Group Enums", _greenButtonStyle, GUILayout.Height(40)))
            {
                GenerateGroupEnums();
            }

            GUI.backgroundColor = new Color(0.1f, 0.4f, 0.1f); // Dark Green
            if (GUILayout.Button("2. Create Audio Enums", _greenButtonStyle, GUILayout.Height(40)))
            {
                GenerateAudioEnums();
            }
            
            GUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.1f); // Dark Yellow/Gold
            if (GUILayout.Button("Clean Audio Scriptables", _yellowButtonStyle, GUILayout.Height(30)))
            {
                RemoveScriptables();
            }
            
            GUI.backgroundColor = defaultColor; // Reset color

            GUILayout.Space(15);

            GUIStyle helpTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
            helpTextStyle.fontSize = 11;
            helpTextStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            EditorGUILayout.LabelField("Follow numbers to setup the Audio Manager", helpTextStyle);

            GUILayout.Space(20);

            float btnHeight = 25;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.Height(btnHeight))) Application.OpenURL("https://github.com/Game-Dojo/Sound-Manager-Advance");
            if (GUILayout.Button("Support", GUILayout.Height(btnHeight))) Application.OpenURL("https://github.com/Game-Dojo/Sound-Manager-Advance/issues");
            EditorGUILayout.EndHorizontal();
        }
        private void DrawPreferencesTab()
        {
            Rect bannerRect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
            GUI.Box(bannerRect, "PREFERENCES", _headerStyle);

            GUILayout.Space(4);
            
            GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 13;
            subHeaderStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            
            _targetMixer = (AudioMixer)EditorGUILayout.ObjectField(
                "Target Mixer",
                _targetMixer,
                typeof(AudioMixer),
                false
            );
            GUILayout.Space(4);
            
            EditorGUILayout.LabelField("Paths/Routes", subHeaderStyle);
            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scripts Path", GUILayout.Width(100));
            _scriptsPath = EditorGUILayout.TextField(_scriptsPath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                _scriptsPath = GetSelectedFolderPath("Scripts", _scriptsPath);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resources Path", GUILayout.Width(100));
            _resourcesPath = EditorGUILayout.TextField(_resourcesPath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                _resourcesPath = GetSelectedFolderPath("Resources", _resourcesPath);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scriptables Path", GUILayout.Width(100));
            _scriptablePath = EditorGUILayout.TextField(_scriptablePath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                _scriptablePath = GetSelectedFolderPath("Scriptables", _scriptablePath);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(8);
            
            EditorGUILayout.LabelField("Fine tuning", subHeaderStyle);
            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fade Curve", GUILayout.Width(100));
            _animationCurve = EditorGUILayout.CurveField(_animationCurve);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(16);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Settings", GUILayout.Height(25)))
            {
                UpdateSettings();
            };
            EditorGUILayout.EndHorizontal();
        }
        #endregion
        
        #region Private Methods
        private void GenerateAudioEnums()
        {
            Editor.AudioEnumGenerator.GenerateEnum();
        }
        private void GenerateGroupEnums()
        {
            Editor.AudioEnumGenerator.GenerateGroupsEnum();
        }
        
        private static void FindAudioSettings()
        {
            AudioSettings mySo = AssetDatabase.LoadAssetAtPath<AudioSettings>(SettingsPath);
            if (mySo)
            {
                _targetMixer = mySo.targetMixer;
                return;
            }
            
            string[] guids = AssetDatabase.FindAssets("AudioSettings t:ScriptableObject");
            if (guids.Length <= 0) return;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AudioSettings settingsSo = AssetDatabase.LoadAssetAtPath<AudioSettings>(path);
            _targetMixer = settingsSo.targetMixer;
        }
        
        private void UpdateSettings()
        {
            AudioSettings mySo = AssetDatabase.LoadAssetAtPath<AudioSettings>(SettingsPath);
            if (!mySo) return;

            mySo.SetMixer(_targetMixer);
            mySo.SetPaths(_scriptsPath, _resourcesPath, _scriptablePath, AudioPath);
                
            EditorUtility.SetDirty(mySo);
            AssetDatabase.SaveAssets();
        }
        private void RemoveScriptables()
        {
            AudioEnumGenerator.CleanAll();
            DeleteScriptables();
        }
        
        #endregion
        
        #region Style
        private void InitStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.box);
            _headerStyle.normal.textColor = Color.white;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.fontSize = 20;
            
            _miniLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            _miniLabelStyle.normal.textColor = Color.white;
            _miniLabelStyle.richText = true;

            _greenButtonStyle = new GUIStyle(GUI.skin.button);
            _greenButtonStyle.normal.textColor = Color.green;
            _greenButtonStyle.fontStyle = FontStyle.Bold;

            _yellowButtonStyle = new GUIStyle(GUI.skin.button);
            _yellowButtonStyle.normal.textColor = Color.yellow;

            _footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        }
        #endregion
        
        #region Static Methods
        private static string GetSelectedFolderPath(string folderName = "Folder", string optional = "")
        {
            var fullPath = EditorUtility.OpenFolderPanel($"Select {folderName} Location", "Assets", "");
            if (string.IsNullOrEmpty(fullPath)) return optional;
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return fullPath.Substring(projectPath.Length + 8);
        }
        private static void DeleteScriptables()
        {
            var scriptablePath = AudioEnumGenerator.GetScriptablePath();
            if (scriptablePath == "")
            {
                Debug.Log("Must set an ScriptableObjects path!");
                return;
            }
            
            string[] guids = AssetDatabase.FindAssets("t:AudioScriptable", new[] { scriptablePath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
            }
        }
        #endregion
    }
}
#endif