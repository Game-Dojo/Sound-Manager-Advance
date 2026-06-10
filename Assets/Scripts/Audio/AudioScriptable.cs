using NaughtyAttributes;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioItem", menuName = "AudioManager/Create Audio item", order = 0)]
    public class AudioScriptable : ScriptableObject 
    {
        [ShowAssetPreview]
        public AudioClip clip;
        
        [Header("Audio Type")]
        public AudioGroupID groupID;
        
        [Header("Variations")]
        [MinMaxSlider(-1f, 2f)]
        public Vector2 volume = new Vector2(1,1);
        
        [MinMaxSlider(-3f, 3f)]
        public Vector2 pitch = new Vector2(1,1);

        [Header("3D Sound")]
        public bool use3DSound = false;

        public bool useCustomCurve = false;
        [ShowIf("use3DSound")][DisableIf("useCustomCurve")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        [ShowIf("use3DSound")][MinMaxSlider(1f, 50f)]
        public Vector2 minMaxDistance = new Vector2(2,50);
        [ShowIf("useCustomCurve")]
        public AnimationCurve distanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });
        
        public void Initialize(AudioClip paramClip)
        {
            this.clip = paramClip;
        }
        
        public AudioClip Get() => clip;
        public AudioGroupID GetID() => groupID;
        public Vector2 GetVolumeRange() => volume;
        public Vector2 GetPitchRange() => pitch;
    }
}