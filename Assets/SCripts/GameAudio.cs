using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton that owns music + one-shot sound effects across the whole game.
// Auto-loads its clips from Resources/Audio so no Inspector wiring is needed.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    // Audio sources are created at runtime so the prefab works headless.
    private AudioSource musicSource;
    private AudioSource sfxSource;

    // Loaded clip cache.
    private AudioClip musicMenu;
    private AudioClip musicGameplay;
    private AudioClip sfxJump;
    private AudioClip sfxSplash;
    private AudioClip sfxYellow;
    private AudioClip sfxBlack;
    private AudioClip sfxGameOver;

    // Build-index of the gameplay scene used by the music switcher.
    // 0 = MainMenu, anything else = gameplay.
    private const int MainMenuBuildIndex = 0;

    // Spawn the singleton automatically before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new("GameAudio");
        Instance = go.AddComponent<GameAudio>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f;

        LoadClips();

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        ApplyMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        ApplyMusicForScene(to);
    }

    private void LoadClips()
    {
        musicMenu      = Resources.Load<AudioClip>("Audio/music_1");
        musicGameplay  = Resources.Load<AudioClip>("Audio/music_2");
        sfxJump        = Resources.Load<AudioClip>("Audio/jump_sound_effect");
        sfxSplash      = Resources.Load<AudioClip>("Audio/water-splash_effect");
        sfxYellow      = Resources.Load<AudioClip>("Audio/Yellow_Collect_Effect");
        sfxBlack       = Resources.Load<AudioClip>("Audio/Black Balloon_Collect");
        sfxGameOver    = Resources.Load<AudioClip>("Audio/Character_Died_Game_Over");
    }

    private void ApplyMusicForScene(Scene scene)
    {
        // The menu is whichever scene sits at build index 0.
        AudioClip target = scene.buildIndex == MainMenuBuildIndex ? musicMenu : musicGameplay;

        // Skip when the requested track is already playing.
        if (target == null || (musicSource.clip == target && musicSource.isPlaying))
        {
            return;
        }

        musicSource.clip = target;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayJump()        => PlayOneShot(sfxJump);
    public void PlaySplash()      => PlayOneShot(sfxSplash);
    public void PlayYellow()      => PlayOneShot(sfxYellow);
    public void PlayBlack()       => PlayOneShot(sfxBlack);
    public void PlayGameOver()    => PlayOneShot(sfxGameOver);

    // Halt the looping background music. Used when the game ends.
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
