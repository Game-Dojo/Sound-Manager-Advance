using Audio;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject locatedSound;
    
    private AudioManager _audioManager;
    
    private void Start()
    {
        _audioManager = AudioManager.Instance;
        _audioManager.OnLoadComplete += SoundLoadCompleted;
    }

    private void OnDestroy()
    {
        _audioManager.OnLoadComplete -= SoundLoadCompleted;
    }

    private void SoundLoadCompleted()
    {
        _audioManager.PlayMusic(AudioID.PuzzleMenu);
        
        if (locatedSound)
            _audioManager.PlayFollow(AudioID.Switch_CON_ESPACIOS, locatedSound);
    }
}