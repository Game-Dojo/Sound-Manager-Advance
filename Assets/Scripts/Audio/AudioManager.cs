using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioSource musicSource;
        [Space(8)] 
        [SerializeField] private string scriptablesPath = "Assets/Scriptables/";
        [SerializeField] private string soundsPath = "Sounds/";
        [SerializeField] private string musicPath = "Music/";

        [Header("Mixer")]
        [SerializeField] private AudioMixer mainMixer;

        [SerializeField] private List<string> groupsList;
        
        [Header("Third Party")]
        [SerializeField] private bool useDoTween = false;
        
        [Header("Events")]
        [Space(5)]
        public UnityEvent onMusicVolumeChanged;
        public UnityEvent onSoundVolumeChanged;
        
        private readonly Dictionary<AudioID, AudioClip> _loadedClips = new Dictionary<AudioID, AudioClip>();
        private readonly Dictionary<AudioID, AudioScriptable> _loadedScriptables = new Dictionary<AudioID, AudioScriptable>();
        
        public static AudioManager Instance { get; private set; }
        
        private List<AudioSource> sourcesPool = new List<AudioSource>();
        
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
            if (mainMixer) AssignMixerGroups();
        }

        #region Public Methods
        public void PlaySound(AudioID id)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            AudioClip clip = scriptable.Get();
            if (clip == null) return;

            var source = GetAudioSource(scriptable.GetID());
            if (!source)
            {
                Debug.LogWarning("Audio Source is null");
                return;
            }
            
            ChangePitchAndVolume(source, scriptable);
            source.PlayOneShot(clip);
        }
        
        public void PlayFlatSoundAt(AudioID id, Vector3 position)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            AudioSource.PlayClipAtPoint(scriptable.Get(), position);
        }

        public void PlaySoundAt(AudioID id, Vector3 position)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            
            /*GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = "OneShot SFX";
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();*/

            var source = GetAudioSource(scriptable.GetID());
            if (!source)
            {
                Debug.LogWarning("Audio Source is null");
                return;
            }
            source.gameObject.SetActive(true);
            
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            
            // TODO: Add to scriptable
            source.minDistance = 2.0f;
            source.maxDistance = 100.0f;
        
            AudioClip clip = scriptable.Get();
            source.clip = clip;
            
            ChangePitchAndVolume(source, scriptable);
            source.PlayOneShot(clip);

            StartCoroutine(nameof(SourceDisable), source);
        }

        private IEnumerator SourceDisable(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length / source.pitch);
            source.gameObject.SetActive(false);
        }
        
        public void PlayMusic(AudioID id)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable))
            {
                Debug.LogWarning("Scriptable not found");
                return;
            }

            AudioClip clip = scriptable.Get();
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip ({id}) not found");
                return;
            }
            
            var source = GetAudioSource(scriptable.GetID());
            if (!source)
            {
                Debug.LogWarning("Audio Source is null");
                return;
            }
            source.clip = clip;
            source.gameObject.SetActive(true);
            
            ChangePitchAndVolume(source, scriptable);
            source.Play();
        }
        
        public void ChangeMusicVolume(float normalizedValue)
        {
            ChangeVolume("MusicVolume", normalizedValue);
            onMusicVolumeChanged?.Invoke();
        }

        public void ChangeSfxVolume(float normalizedValue)
        {
            ChangeVolume("SFXVolume", normalizedValue);
            onSoundVolumeChanged?.Invoke();
        }

        public void FadeOutMusic()
        {
            
        }

        public void FadeInMusic()
        {
            
        }
        #endregion

        private IEnumerator Fade()
        {
            yield return new WaitForSeconds(0.1f);
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
            string path = $"{scriptablesPath}{clip.name}.asset";
            AudioScriptable mySO = AssetDatabase.LoadAssetAtPath<AudioScriptable>(path);
            if (mySO)
            {
                _loadedScriptables.TryAdd(audioID, mySO);
                mySO.Initialize(clip);
                EditorUtility.SetDirty(mySO);
                AssetDatabase.SaveAssets();
            }
        }
        private void AssignMixerGroups()
        {
            var allGroups = mainMixer.FindMatchingGroups("");

            foreach (AudioMixerGroup group in allGroups)
            {
                var groupName = group.ToString();
                if (groupName != "Master")
                    sourcesPool.Add(CreatePoolAudioSource(groupName));
            }
        }
        
        private AudioSource GetAudioSource(AudioGroupID id)
        {
            foreach (var source in sourcesPool)
            {
                var mixerGroup = source.outputAudioMixerGroup;
                if (mixerGroup.name == id.ToString())
                    return source;
            }

            return null;
        }
        
        private AudioSource CreatePoolAudioSource(string groupName)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = groupName + "Source";
            go.transform.position = Vector3.zero;

            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = mainMixer.FindMatchingGroups(groupName)[0];
            source.playOnAwake = false;

            go.SetActive(false);
            return source;
        }
        #endregion
    }
}
