using Audio;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    private AudioManager _audioManager;
    private Rigidbody _rb;
    private bool _shouldJump = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _audioManager = AudioManager.Instance;
        _audioManager.PlayMusic(AudioID.PuzzleMenu);
    }

    private void Update()
    {
        if (!Keyboard.current[Key.Space].wasPressedThisFrame) return;
        Jump();
    }

    private void FixedUpdate()
    {
        if (_shouldJump)
        {
            _rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            _shouldJump = false;
        }
    }

    private void Jump()
    {
        _shouldJump = true;
        _audioManager.PlaySoundAt(AudioID.Switch, transform.position + Vector3.forward * 5);
    }
}
