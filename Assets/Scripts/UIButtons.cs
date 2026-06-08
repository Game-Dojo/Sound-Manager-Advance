using Audio;
using UnityEngine;

public class UIButtons : MonoBehaviour
{
    public void SoundOne()
    {
        AudioManager.Instance.PlayUISound(AudioID.Click, AudioManager.AudioType.Modified);
    }
    public void SoundTwo()
    {
        AudioManager.Instance.PlayUISound(AudioID.Rollover, AudioManager.AudioType.Modified);
    }
    public void SoundThree()
    {
        AudioManager.Instance.PlayUISound(AudioID.MouseRelease, AudioManager.AudioType.Modified);
    }

    public void FadeMusic()
    {
        AudioManager.Instance.FadeMusic(2.0f);
    }
    
    public void FadeMusicIn()
    {
        AudioManager.Instance.FadeMusic(2.0f, AudioManager.FadeType.FadeIn);
    }
}
