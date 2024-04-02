using System.Reflection;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip[] _pistolFireClips;
    [SerializeField] AudioClip[] _arrowFireClips;
    [SerializeField] AudioClip[] _dashSoundClips;
    [SerializeField] AudioClip[] _meleeAttackClips;
    [SerializeField] AudioClip[] _tinySlugDeathClips;
    [SerializeField] AudioClip[] _coinPickupClips;
    [SerializeField] AudioClip[] _satelliteWeaponEatenClips;
    [SerializeField] AudioClip[] _satelliteWeaponDeathCryClips;

    public static AudioManager Instance { get; private set; }

    AudioSource _pistolFireSource;
    AudioSource _arrowFireSource;
    AudioSource _dashSoundSource;
    AudioSource _meleeAttackSource;
    AudioSource _tinySlugDeathSource;
    AudioSource _coinPickupSource;
    AudioSource _satelliteWeaponEatenSource;
    AudioSource _satelliteWeaponDeathCrySource;

    public enum SFX
    {
        None,
        PistolFire,
        ArrowFire,
        Dash,
        MeleeAttack,
        TinySlugDeath,
        CoinPickup,
        SatelliteWeaponEaten,
        SatelliteWeaponDeathCry,
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
        CreateAudioSourceChild(out _pistolFireSource, "PistolFireSource", 0.3f);
        CreateAudioSourceChild(out _arrowFireSource, "ArrowFireSource", 0.3f);
        CreateAudioSourceChild(out _dashSoundSource, "DashSoundSource");
        CreateAudioSourceChild(out _meleeAttackSource, "MeleeAttackSource");
        CreateAudioSourceChild(out _tinySlugDeathSource, "TinySlugDeathSource", 0.5f);
        CreateAudioSourceChild(out _coinPickupSource, "CoinPickupSource", 0.5f);
        CreateAudioSourceChild(out _satelliteWeaponEatenSource, "SatelliteWeaponEatenSource");
        CreateAudioSourceChild(out _satelliteWeaponDeathCrySource, "SatelliteWeaponDeathCrySource");
    }

    void CreateAudioSourceChild(out AudioSource audioSource, string audioSourceName, float volume = 1.0f)
    {
        GameObject audioSourceGO = new GameObject(audioSourceName);
        audioSource = audioSourceGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.transform.parent = transform;
        audioSource.volume = volume;
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
            case SFX.MeleeAttack: PlayRandomSoundFromClips(_meleeAttackSource, _meleeAttackClips); break;
            case SFX.TinySlugDeath: PlayRandomSoundFromClips(_tinySlugDeathSource, _tinySlugDeathClips); break;
            case SFX.CoinPickup: PlayRandomSoundFromClips(_coinPickupSource, _coinPickupClips); break;
            case SFX.SatelliteWeaponEaten: PlayRandomSoundFromClips(_satelliteWeaponEatenSource, _satelliteWeaponEatenClips); break;
            case SFX.SatelliteWeaponDeathCry: PlayRandomSoundFromClips(_satelliteWeaponDeathCrySource, _satelliteWeaponDeathCryClips); break;
            default: break;
        }
    }
}
