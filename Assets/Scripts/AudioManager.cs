using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip throwClip;
    public AudioClip hitClip;
    public AudioClip captureClip;
    public AudioClip successClip;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public void PlayThrow()
    {
        if (throwClip != null)
            sfxSource.PlayOneShot(throwClip, 0.9f);
    }

    public void PlayHit()
    {
        if (hitClip != null)
            sfxSource.PlayOneShot(hitClip, 1.0f);
    }

    public void PlayCapture()
    {
        if (captureClip != null)
            sfxSource.PlayOneShot(captureClip, 0.95f);
    }

    public void PlaySuccess()
    {
        if (successClip != null)
            sfxSource.PlayOneShot(successClip, 1.0f);
    }
}
