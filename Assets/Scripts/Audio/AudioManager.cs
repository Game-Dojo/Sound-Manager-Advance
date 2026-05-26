using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioSource musicSource;
        [Space(8)]
        [SerializeField] private string soundsPath = "Sounds/";
        [SerializeField] private string musicPath = "Music/";
        
        [Header("Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        
        [Header("Settings")]
        [SerializeField] private bool useDoTween = false;
        
        [Header("Events")]
        [Space(5)]
        public UnityEvent onMusicVolumeChanged;
        public UnityEvent onSoundVolumeChanged;
        
        private readonly Dictionary<AudioID, AudioClip> _loadedClips = new Dictionary<AudioID, AudioClip>();
        private readonly Dictionary<AudioID, AudioScriptable> _loadedScriptables = new Dictionary<AudioID, AudioScriptable>();
        
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("AudioManager is a Singleton: An instance already exists");
                Destroy(this.gameObject);
                return;
            }
        
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            SaveClips();
        }
        
        public void PlaySound(AudioID id)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            AudioClip clip = scriptable.Get();
            if (clip == null) return;
            
            ChangePitchAndVolume(soundSource, scriptable);
            soundSource.PlayOneShot(clip);
        }
        
        public void PlayFlatSoundAt(AudioID id, Vector3 position)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            AudioSource.PlayClipAtPoint(scriptable.Get(), position);
        }

        public void PlaySoundAt(AudioID id, Vector3 position)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = "OneShot SFX";
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            
            // TODO: Add to scriptable
            source.minDistance = 2.0f;
            source.maxDistance = 100.0f;
        
            AudioClip clip = scriptable.Get();
            ChangePitchAndVolume(source, scriptable);
            source.PlayOneShot(clip);

            Destroy(go, clip.length / source.pitch);
        }

        public void ChangeMusicVolume(float sliderValue)
        {
            ChangeVolume("MusicVolume", sliderValue);
            onMusicVolumeChanged?.Invoke();
        }

        public void ChangeSfxVolume(float sliderValue)
        {
            ChangeVolume("SFXVolume", sliderValue);
            onSoundVolumeChanged?.Invoke();
        }
        
        #region Utilities
        private void ChangeVolume(string mixerGroup, float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            mainMixer.SetFloat(mixerGroup, db);
        }
        
        private void ChangePitchAndVolume(AudioSource source, AudioScriptable audioScript)
        {
            source.volume = Random.Range(audioScript.volume.x, audioScript.volume.y);
            source.pitch = Random.Range(audioScript.pitch.x, audioScript.pitch.y);
        }
        
        private void SaveClips()
        {
            AudioClip[] allClips = Resources.LoadAll<AudioClip>("Audio");

            var enumIndex = 1;
            foreach (var clip in allClips)
            {
                _loadedClips.TryAdd((AudioID) enumIndex, clip);
                EditScriptable(clip, (AudioID) enumIndex);
                enumIndex++;
            }
        }

        private void EditScriptable( AudioClip clip, AudioID audioID )
        {
            string path = $"Assets/Scriptables/{clip.name}.asset";
            AudioScriptable mySO = AssetDatabase.LoadAssetAtPath<AudioScriptable>(path);
            _loadedScriptables.TryAdd(audioID, mySO);
            mySO.Initialize(clip);
            EditorUtility.SetDirty(mySO);
            AssetDatabase.SaveAssets();
        }
        #endregion
    }
}
