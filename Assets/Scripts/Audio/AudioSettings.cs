using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "AudioManager/Audio Setting", order = 0)]
    public class AudioSettings : ScriptableObject
    {
        [Header("Paths")] 
        public string scriptsAudioPath = "Assets/Scripts/Audio/";
        public string audioResourcesPath = "Assets/Resources/Audio/";
        public string scriptablesPath = "Assets/Scriptables/";
        public string audioPath = "Audio";
        
        [Header("Curves")]
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public void SetPaths(string scrPath, string resourcePath, string scriptables, string audio)
        {
            scriptsAudioPath = $"Assets/{scrPath}/{audio}/";
            audioResourcesPath = $"Assets/{resourcePath}/{audio}/";
            scriptablesPath = $"Assets/{scriptables}/";
            audioPath = audio;
        }
    }
}