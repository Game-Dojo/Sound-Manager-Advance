using UnityEditor;
using UnityEngine;

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
        
        private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private string _resourcesPath = "Resources";
        private string _scriptablePath = "Scriptables";
        private string _scriptsPath = "Scripts";
        private string _audioPath = "Audio";
        
        [MenuItem("Tools/Audio Manager")]
        public static void ShowWindow()
        {
            AudioSetupWindow window = GetWindow<AudioSetupWindow>("Audio Manager Panel");
            window.minSize = new Vector2(300, 500);
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

        private void InitStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.box);
            _headerStyle.normal.textColor = Color.white;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.fontSize = 20;

            _greenButtonStyle = new GUIStyle(GUI.skin.button);
            _greenButtonStyle.normal.textColor = Color.green;
            _greenButtonStyle.fontStyle = FontStyle.Bold;

            _yellowButtonStyle = new GUIStyle(GUI.skin.button);
            _yellowButtonStyle.normal.textColor = Color.yellow;

            _footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        }
        private void DrawSetupTab()
        {
            Rect bannerRect = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            GUI.Box(bannerRect, "AUDIO MANAGER", _headerStyle);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("AudioManager v0.0.1 [Release build]", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Group Enums: " + (AudioEnumGenerator.IsGroupEnumCreated() ? "Created!" : "not created"), EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Audio Enums: " + (AudioEnumGenerator.IsAudioEnumCreated() ? "Created!" : "not created"), EditorStyles.miniLabel);

            GUILayout.Space(15);

            Color defaultColor = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.1f, 0.4f, 0.1f); // Dark Green
            if (GUILayout.Button("1. Create Group Enums", _greenButtonStyle, GUILayout.Height(40)))
            {
                Debug.Log("Generate Group Enums");
                Editor.AudioEnumGenerator.GenerateGroupsEnum();
            }

            GUI.backgroundColor = new Color(0.1f, 0.4f, 0.1f); // Dark Green
            if (GUILayout.Button("2. Create Audio Enums", _greenButtonStyle, GUILayout.Height(40)))
            {
                Debug.Log("Generate Audio Enums");
                Editor.AudioEnumGenerator.GenerateEnum();
            }
            
            GUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.1f); // Dark Yellow/Gold
            if (GUILayout.Button("Clean Audio Scriptables", _yellowButtonStyle, GUILayout.Height(30)))
            {
                Debug.Log("Clean all scriptable audio assets");
                AudioEnumGenerator.CleanAll();
                DeleteScriptables();
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
            
            EditorGUILayout.LabelField("Paths/Routes", subHeaderStyle);
            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scripts Path", GUILayout.Width(100));
            _scriptsPath = EditorGUILayout.TextField(_scriptsPath);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resources Path", GUILayout.Width(100));
            _resourcesPath = EditorGUILayout.TextField(_resourcesPath);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scriptables Path", GUILayout.Width(100));
            _scriptablePath = EditorGUILayout.TextField(_scriptablePath);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio Path", GUILayout.Width(100));
            _audioPath = EditorGUILayout.TextField(_audioPath);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Fade Curve", GUILayout.Width(100));
            _animationCurve = EditorGUILayout.CurveField(_animationCurve);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(16);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Settings", GUILayout.Height(25)))
            {
                Debug.Log("Save Settings");
                UpdateSettings();
            };
            EditorGUILayout.EndHorizontal();
        }
        
        private static void DeleteScriptables()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioScriptable", new[] { "Assets/Scriptables" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
            }
        }

        private void UpdateSettings()
        {
            string path = $"Assets/AudioSettings.asset";
            Audio.AudioSettings mySo = AssetDatabase.LoadAssetAtPath<Audio.AudioSettings>(path);
            if (!mySo) return;

            //mySo.scriptsAudioPath = $"Assets/{_scriptsPath}/{_audioPath}";
            //mySo.audioResourcesPath = $"Assets/{_resourcesPath}/{_audioPath}";
            //mySo.scriptablesPath = $"Assets/{_scriptablePath}/{_audioPath}";
            //mySo.audioPath = $"{_audioPath}";
            
            mySo.SetPaths(_scriptsPath, _resourcesPath, _scriptablePath, _audioPath);
                
            EditorUtility.SetDirty(mySo);
            AssetDatabase.SaveAssets();
        }

    }
}