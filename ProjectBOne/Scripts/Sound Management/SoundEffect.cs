using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Set The Sound That Will Be Played
    /// </summary>
    public void SetSound(SoundEffectSO soundEffect)
    {
        audioSource.pitch = Random.Range(soundEffect.soundEffectMinRandomValuePitch,
            soundEffect.soundEffectMaxRandomValuePitch);
        
        audioSource.volume = soundEffect.soundEffectVolume;

        int i = Random.Range(0, soundEffect.soundEffectClip.Length);

        audioSource.clip = soundEffect.soundEffectClip[i];
    }
}
