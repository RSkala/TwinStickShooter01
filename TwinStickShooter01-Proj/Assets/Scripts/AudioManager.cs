using System.Reflection;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip[] _pistolFireClips;
    [SerializeField] AudioClip[] _arrowFireClips;
    [SerializeField] AudioClip[] _dashSoundClips;

    public static AudioManager Instance { get; private set; }

    AudioSource _pistolFireSource;
    AudioSource _arrowFireSource;
    AudioSource _dashSoundSource;

    public enum SFX
    {
        None,
        PistolFire,
        ArrowFire,
        Dash
    }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        // Create the AudioSource objects and add as chidren
        CreateAudioSourceChild(out _pistolFireSource, "PistolFireSource");
        CreateAudioSourceChild(out _arrowFireSource, "ArrowFireSource");
        CreateAudioSourceChild(out _dashSoundSource, "DashSoundSource");
    }

    void CreateAudioSourceChild(out AudioSource audioSource, string audioSourceName)
    {
        GameObject audioSourceGO = new GameObject(audioSourceName);
        audioSource = audioSourceGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.transform.parent = transform;
    }

    void PlayRandomSoundFromClips(AudioSource audioSource, AudioClip[] audioClips, bool stopIfPlaying = true)
    {
        if(audioClips.Length <= 0)
        {
            return;
        }

        if(stopIfPlaying && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Randomly select a clip, set the clip in the AudioSource, then play it
        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void PlaySound(SFX sfx)
    {
        switch(sfx)
        {
            case SFX.PistolFire: PlayRandomSoundFromClips(_pistolFireSource, _pistolFireClips); break;
            case SFX.ArrowFire: PlayRandomSoundFromClips(_arrowFireSource, _arrowFireClips); break;
            case SFX.Dash: PlayRandomSoundFromClips(_dashSoundSource, _dashSoundClips); break;
            default: break;
        }
    }
}
