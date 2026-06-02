using NaughtyAttributes;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioItem", menuName = "AudioManager/Create Audio item", order = 0)]
    public class AudioScriptable : ScriptableObject 
    {
        public AudioClip clip;
        
        [Header("Audio Type")]
        public bool isMusic = false;
        
        [Header("Variations")]
        [MinMaxSlider(-1f, 2f)]
        public Vector2 volume = new Vector2(1,1);
        
        [MinMaxSlider(-3f, 3f)]
        public Vector2 pitch = new Vector2(1,1);

        public void Initialize(AudioClip paramClip)
        {
            this.clip = paramClip;
        }
        
        public AudioClip Get() => clip;
        public Vector2 GetVolumeRange() => volume;
        public Vector2 GetPitchRange() => pitch;
    }
}