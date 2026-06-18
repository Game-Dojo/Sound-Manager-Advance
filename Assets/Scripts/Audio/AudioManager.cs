using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AudioSettings audioSettings;
        [SerializeField] private AudioMixer mainMixer;
        
        [Header("Events")]
        [Space(5)]
        [SerializeField] private UnityEvent onMusicVolumeChanged;
        [SerializeField] private UnityEvent onSoundVolumeChanged;
        
        #region Private members
        private readonly Dictionary<AudioID, AudioClip> _loadedClips = new Dictionary<AudioID, AudioClip>();
        private readonly Dictionary<AudioID, AudioScriptable> _loadedScriptables = new Dictionary<AudioID, AudioScriptable>();
        private  List<AudioSource> _sourcesPool = new List<AudioSource>();
        
        private Coroutine _disableSourceRoutine;
        private Coroutine _activeFadeRoutine;
        
        private const string MusicExposed = "MusicVolume";

        private int _assetsToLoad = 0;
        private int _loadedAssets = 0;
        #endregion
        
        public enum AudioMode
        {
            Flat,
            Modified
        }
        public enum FadeType
        {
            FadeIn,
            FadeOut
        }
        
        public static AudioManager Instance { get; private set; }
        
        public event Action OnLoadComplete;
        
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
            
            Addressables.LoadAssetAsync<AudioSettings>("AudioSettings").Completed += (handle) => {
                mainMixer = (handle.Result).targetMixer;
                AssignMixerGroups();
            };
        }

        #region Play Methods
        public void PlaySound(AudioID id, bool loop = false, AudioMode mode = AudioMode.Flat)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            
            AudioClip clip = scriptable.Get();
            if (clip == null) return;

            var source = GetFreeSource();
            if (!source)
            {
                Debug.LogWarning("Audio Source is null");
                return;
            }
            
            if (scriptable.groupID != AudioGroupID.None)
                source.outputAudioMixerGroup = mainMixer.FindMatchingGroups(scriptable.groupID.ToString())[0];
            
            source.gameObject.SetActive(true);
            
            if (mode != AudioMode.Flat)
                ChangePitchAndVolume(source, scriptable);
            
            if (loop)
            {
                source.clip = clip;
                source.loop = true;
                source.Play();
                return;
            }
            
            source.PlayOneShot(clip);
            if (_disableSourceRoutine != null) StopCoroutine(_disableSourceRoutine);
            _disableSourceRoutine = StartCoroutine(nameof(SourceDisable), source);
        }
        public void PlaySoundAt(AudioID id, Vector3 position)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            AudioSource.PlayClipAtPoint(scriptable.Get(), position);
        }
        public void PlaySoundAt(AudioID id, Vector3 position, AudioMode mode)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            
            var source = GetAudioSource(scriptable.GetID());
            if (!source)
            {
                Debug.LogWarning("Audio Source is null");
                return;
            }
            source.gameObject.SetActive(true);
            
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2.0f;
            source.maxDistance = 50.0f;

            if (scriptable.use3DSound)
            {
                source.rolloffMode = scriptable.rolloffMode;
                source.minDistance = scriptable.minMaxDistance.x;
                source.maxDistance = scriptable.minMaxDistance.y;
            }
        
            AudioClip clip = scriptable.Get();
            source.clip = clip;
            
            if (mode == AudioMode.Modified) ChangePitchAndVolume(source, scriptable);
            source.PlayOneShot(clip);

            if (_disableSourceRoutine != null) StopCoroutine(_disableSourceRoutine);
            _disableSourceRoutine = StartCoroutine(nameof(SourceDisable), source);
        }
        public void PlayUISound(AudioID id, AudioMode mode = AudioMode.Flat)
        {
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable)) return;
            var source = GetAudioSource(scriptable.GetID());
            if (!source)
            {
                Debug.LogWarning("UI Source is null");
                return;
            }
            
            source.gameObject.SetActive(true);
            source.spatialBlend = 0f;
            
            AudioClip clip = scriptable.Get();
            if (clip == null)
            {
                Debug.LogWarning("AudioClip is null");
                return;
            }
            source.clip = clip;
            
            if (mode == AudioMode.Modified) ChangePitchAndVolume(source, scriptable);
            source.PlayOneShot(clip);
            
            if (_disableSourceRoutine != null) StopCoroutine(_disableSourceRoutine);
            _disableSourceRoutine = StartCoroutine(nameof(SourceDisable), source);
        }
        public void PlayMusic(AudioID id, bool loopMode = true)
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
                Debug.LogWarning($"Audio Source ({scriptable.GetID()}) is null");
                return;
            }
            
            source.clip = clip;
            source.gameObject.SetActive(true);
            source.loop = loopMode;
            
            ChangePitchAndVolume(source, scriptable);
            source.Play();
        }
        public void PlayFollow(AudioID id, GameObject objectToFollow, bool loopMode = true)
        {
            AudioSource source = GetFreeSource();
            if (source == null) return;
            
            if (!_loadedScriptables.TryGetValue(id, out AudioScriptable scriptable))
            {
                Debug.LogError("Scriptable not found");
                return;
            }
            
            AudioClip clip = scriptable.Get();
            if (clip == null)
            {
                Debug.LogError($"AudioClip ({id}) not found");
                return;
            }
            
            source.clip = clip;
            source.loop = loopMode;
            
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0;
            source.minDistance = 2.0f;
            source.maxDistance = 50.0f;
            
            if (scriptable.groupID != AudioGroupID.None)
            {
                source.outputAudioMixerGroup = mainMixer.FindMatchingGroups(scriptable.groupID.ToString())[0];
            }
            
            if (scriptable.use3DSound)
            {
                source.rolloffMode = scriptable.rolloffMode;
                source.minDistance = scriptable.minMaxDistance.x;
                source.maxDistance = scriptable.minMaxDistance.y;
                
                if (scriptable.useCustomCurve)
                    source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, scriptable.distanceCurve);
            }
            
            source.gameObject.SetActive(true);
            source.transform.SetParent(objectToFollow.transform);
            source.Play();
        }
        #endregion
        
        #region Modify Methods
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
        public void FadeMusic(float duration = 1.0f, FadeType type = FadeType.FadeOut)
        {
            if (_activeFadeRoutine != null) StopCoroutine(_activeFadeRoutine);
            _activeFadeRoutine = StartCoroutine(FadeRoutine((type == FadeType.FadeOut) ? 0 : 1.0f, duration));
        }
        #endregion
        
        #region Utilities
        private void ChangeVolume(string mixerGroup, float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 10f)) * 20;
            mainMixer.SetFloat(mixerGroup, db);
        }
        private void ChangePitchAndVolume(AudioSource source, AudioScriptable audioScript)
        {
            source.volume = Random.Range(audioScript.volume.x, audioScript.volume.y);
            source.pitch = Random.Range(audioScript.pitch.x, audioScript.pitch.y);
        }
        private IEnumerator SourceDisable(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length / source.pitch);
            source.gameObject.SetActive(false);
        }
        private IEnumerator FadeRoutine(float targetLinear, float duration)
        {
            float elapsed = 0f;

            mainMixer.GetFloat(MusicExposed, out float currentDb);
            float startLinear = Mathf.Pow(10, currentDb / 20);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
            
                float percentage = Mathf.Clamp01(elapsed / duration);
                float curveWeight = audioSettings.fadeCurve.Evaluate(percentage);
                float currentVolume = Mathf.Lerp(startLinear, targetLinear, curveWeight);
                
                ChangeVolume(MusicExposed, currentVolume);

                yield return null;
            }

            ChangeVolume(MusicExposed, targetLinear);
        }
        #endregion
        
        #region Sources
        private AudioSource GetAudioSource(AudioGroupID id)
        {
            foreach (var source in _sourcesPool)
            {
                var mixerGroup = source.outputAudioMixerGroup;
                if (mixerGroup && mixerGroup.name == id.ToString())
                    return source;
            }

            return null;
        }
        private AudioSource GetFreeSource()
        {
            foreach (var source in _sourcesPool)
            {
                var mixerGroup = source.outputAudioMixerGroup;
                if (mixerGroup == null) return source;
            }

            return null;
        }
        private AudioSource CreatePoolAudioSource(string groupName, AudioMixerGroup group)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = groupName + "Source";
            go.transform.position = Vector3.zero;

            AudioSource source = go.AddComponent<AudioSource>();
            if (group)
            {
                source.outputAudioMixerGroup = group; //mainMixer.FindMatchingGroups(groupName)[0];
            }
            source.playOnAwake = false;

            go.SetActive(false);
            return source;
        }
        #endregion
        
        #region Loading + Handling
        private void SaveClips()
        {
            if (!audioSettings)
            {
                Debug.LogWarning("Audio settings not set");
                return;
            }
            
            AudioClip[] allClips = Resources.LoadAll<AudioClip>(audioSettings.audioPath);
            _assetsToLoad = allClips.Length;
            
            foreach (var clip in allClips)
            {
                var clipName = clip.name.Replace(" ", "_");
                if (!Enum.TryParse(clipName, out AudioID id)) continue;
                
                _loadedClips.TryAdd(id, clip);
                EditScriptable(clip, id);
            }
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
        private void EditScriptable( AudioClip clip, AudioID audioID )
        {
            if (!audioSettings)
            {
                Debug.LogWarning("Audio settings not set");
                return;
            }
            
            Addressables.LoadAssetAsync<AudioScriptable>(audioID.ToString()).Completed += (handle) => {
                AudioScriptable mySo = handle.Result as AudioScriptable;
                mySo.Initialize(clip);
                
                _loadedScriptables.TryAdd(audioID, mySo);
                _loadedAssets += 1;

                if (_loadedAssets >= _assetsToLoad)
                    StartCoroutine(DelayedInvoke());
#if UNITY_EDITOR
                EditorUtility.SetDirty(mySo);
#endif
            };
        }

        private IEnumerator DelayedInvoke()
        {
            yield return null;
            OnLoadComplete?.Invoke();
        }

        private void AssignMixerGroups()
        {
            var allGroups = mainMixer.FindMatchingGroups("");
            
            foreach (AudioMixerGroup group in allGroups)
            {
                var groupName = group.ToString();

                if (groupName == "Master") continue;
                _sourcesPool.Add(CreatePoolAudioSource(groupName, group));
            }

            for (var i = 0; i < 10; i++)
            {
                _sourcesPool.Add(CreatePoolAudioSource($"Audio_{i+1}", null));
            }
        }
        #endregion
    }
}
