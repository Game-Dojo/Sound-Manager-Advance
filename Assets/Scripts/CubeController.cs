using Audio;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
            AudioManager.Instance.PlaySoundAt(AudioID.Switch, transform.position + Vector3.forward * 5);
    }
}
