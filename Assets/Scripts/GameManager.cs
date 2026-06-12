using System.Collections;
using Audio;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject locatedSound;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        
        //AudioManager.Instance.PlaySound(AudioID.PRUEBA_SONIDO, true, AudioManager.AudioMode.Flat);
        
        if (locatedSound)
            AudioManager.Instance.PlayFollow(AudioID.Switch_CON_ESPACIOS, locatedSound);
    }
}
