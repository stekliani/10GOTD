using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, ISoundsObserver
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Sounds")]
    [SerializeField] private AudioClip arrowLaunchSound;
    [SerializeField] private AudioClip arrowHitSound;
    [SerializeField] private AudioClip rocketLaunchSound;
    [SerializeField] private AudioClip rocketHitSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip levelUpSound;


    [Header("Background Musics")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip UIMusic;

    [Header("Launch Settings")]
    [SerializeField] private int launchPoolSize = 5;            // Number of AudioSources for rocket launches

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup BackgrounMusicGroup;
    [SerializeField] private AudioMixerGroup UIMusicGroup;

    private List<AudioSource> rocketLaunchSources = new List<AudioSource>();
    private AudioSource rocketHitSource;
    private AudioSource clickSource;
    private AudioSource levelUpSource;
    private AudioSource arrowLaunchSource;
    private AudioSource arrowHitSource;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAllSources();

    }

    #region PlaySound Functions
    /// <summary>
    /// Play rocket launch with overlapping support and optional pitch variation
    /// </summary>
    public void PlayRocketLaunch()
    {
        if (rocketLaunchSound == null) return;

        AudioSource src = rocketLaunchSources.Find(s => !s.isPlaying) ?? rocketLaunchSources[0];

        src.Stop();

        // Random pitch
        src.pitch = 1f + Random.Range(-0.1f, 0.15f);

        // Random volume
        src.volume = 0.9f + Random.Range(-0.1f, 0.1f);

        // Optional: small stereo pan
        src.panStereo = Random.Range(-0.2f, 0.2f);

        src.Play();
    }

    /// <summary>
    /// Play rocket hit (single source)
    /// </summary>
    public void PlayRocketHit()
    {
        if (rocketHitSound == null) return;

        rocketHitSource.Stop();
        rocketHitSource.Play();
    }

    public void PlayClick()
    {
        if (clickSound == null) return;
        clickSource.Stop();
        clickSource.Play();
    }

    public void playOnLevelUp()
    {
        if (levelUpSound == null) return;

        levelUpSource.Stop();
        levelUpSource.Play();
    }

    public void PlayArrowLaunch()
    {
        if (arrowLaunchSound == null) return;
        arrowLaunchSource.Stop();
        arrowLaunchSource.Play();
    }

    public void PlayArrowHit()
    {
        if (arrowHitSound == null) return;
        arrowHitSource.Stop();
        arrowHitSource.Play();
    }

    #endregion

    /// <summary>
    /// Event observer
    /// </summary>
    public void OnNotify(SoundActions action)
    {
        switch (action)
        {
            case SoundActions.none:
                return;
            case SoundActions.playArrowLaunch:
                PlayArrowLaunch();
                break;
            case SoundActions.playArrowHit:
                PlayArrowHit();
                break;
            case SoundActions.playRocketLaunch:
                PlayRocketLaunch();
                break;

            case SoundActions.playRocketHit:
                PlayRocketHit();
                break;
            case SoundActions.playOnLevelUp:
                playOnLevelUp();
                break;
            case SoundActions.playClick:
                PlayClick();
                break;
        }
    }

    #region Initialization
    public void InitializeLaunchSources()
    {
        for (int i = 0; i < launchPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.clip = rocketLaunchSound;
            src.outputAudioMixerGroup = sfxGroup;
            rocketLaunchSources.Add(src);
        }
    }

    public void InitializeHitSource()
    {
        // Single AudioSource for hits
        rocketHitSource = gameObject.AddComponent<AudioSource>();
        rocketHitSource.playOnAwake = false;
        rocketHitSource.clip = rocketHitSound;
        rocketHitSource.outputAudioMixerGroup = sfxGroup;
    }

    public void InitializeClickSource()
    {
        // Single AudioSource for hits
        clickSource = gameObject.AddComponent<AudioSource>();
        clickSource.playOnAwake = false;
        clickSource.clip = clickSound;
        clickSource.outputAudioMixerGroup = UIMusicGroup;
    }

    public void InitializeOnLevelUpSource()
    {
        // Single AudioSource for hits
        levelUpSource = gameObject.AddComponent<AudioSource>();
        levelUpSource.playOnAwake = false;
        levelUpSource.clip = levelUpSound;
        levelUpSource.outputAudioMixerGroup = sfxGroup;
    }

    public void InitializeArrowLaunchSource()
    {
        arrowLaunchSource = gameObject.AddComponent<AudioSource>();
        arrowLaunchSource.playOnAwake = false;
        arrowLaunchSource.clip = arrowLaunchSound;
        arrowLaunchSource.outputAudioMixerGroup = sfxGroup;
    }

    public void InitializeArrowHitSource()
    {
        arrowHitSource = gameObject.AddComponent<AudioSource>();
        arrowHitSource.playOnAwake = false;
        arrowHitSource.clip = arrowHitSound;
        arrowHitSource.outputAudioMixerGroup = sfxGroup;
    }
    #endregion

    public void InitializeAllSources()
    {
        InitializeLaunchSources();
        InitializeHitSource();
        InitializeClickSource();
        InitializeOnLevelUpSource();
        InitializeArrowLaunchSource();
        InitializeArrowHitSource();
    }
    private void OnEnable()
    {
        SoundEventBus.OnSoundEvent += OnNotify;
    }

    private void OnDisable()
    {
        SoundEventBus.OnSoundEvent -= OnNotify;
    }
}