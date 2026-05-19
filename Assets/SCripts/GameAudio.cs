// Bring in core Unity types like MonoBehaviour and AudioSource.
using UnityEngine;
// Bring in scene-management types for tracking the active scene.
using UnityEngine.SceneManagement;

// Singleton that owns music + one-shot sound effects across the whole game.
// Auto-loads its clips from Resources/Audio so no Inspector wiring is needed.
public class GameAudio : MonoBehaviour
{
    // Store the shared singleton instance accessible to the rest of the game.
    public static GameAudio Instance { get; private set; }

    // Audio sources are created at runtime so the prefab works headless.
    // Store the looping audio source used for background music.
    private AudioSource musicSource;
    // Store the audio source used for one-shot sound effects.
    private AudioSource sfxSource;

    // Loaded clip cache.
    // Store the menu music clip.
    private AudioClip musicMenu;
    // Store the gameplay music clip.
    private AudioClip musicGameplay;
    // Store the jump sound effect clip.
    private AudioClip sfxJump;
    // Store the splash sound effect clip.
    private AudioClip sfxSplash;
    // Store the yellow-balloon collect sound effect clip.
    private AudioClip sfxYellow;
    // Store the black-balloon collect sound effect clip.
    private AudioClip sfxBlack;
    // Store the game-over sound effect clip.
    private AudioClip sfxGameOver;

    // Build-index of the gameplay scene used by the music switcher.
    // 0 = MainMenu, anything else = gameplay.
    private const int MainMenuBuildIndex = 0;

    // Spawn the singleton automatically before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        // Stop if the singleton already exists.
        if (Instance != null)
        {
            return;
        }

        // Create the host GameObject that will hold the audio behavior.
        GameObject go = new("GameAudio");
        // Add the GameAudio component and remember it as the singleton.
        Instance = go.AddComponent<GameAudio>();
        // Keep the audio host alive across scene loads.
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        // Destroy this duplicate when a different singleton already exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register this object as the active singleton.
        Instance = this;

        // Create the dedicated music audio source.
        musicSource = gameObject.AddComponent<AudioSource>();
        // Loop background music continuously.
        musicSource.loop = true;
        // Do not auto-play on awake; the scene logic decides when to start.
        musicSource.playOnAwake = false;
        // Set a moderate music volume so it sits under sound effects.
        musicSource.volume = 0.5f;

        // Create the dedicated sound-effects audio source.
        sfxSource = gameObject.AddComponent<AudioSource>();
        // SFX clips never loop.
        sfxSource.loop = false;
        // Do not auto-play on awake; effects fire on demand.
        sfxSource.playOnAwake = false;
        // Set full volume for sound effects.
        sfxSource.volume = 1f;

        // Pre-load every audio clip from the Resources folder.
        LoadClips();

        // Subscribe to scene-change events so the music can switch tracks.
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid leaking the event handler.
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        // Apply the correct music for the scene that is currently active.
        ApplyMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        // Switch music to match the new active scene.
        ApplyMusicForScene(to);
    }

    private void LoadClips()
    {
        // Load the menu music clip from Resources.
        musicMenu      = Resources.Load<AudioClip>("Audio/music_1");
        // Load the gameplay music clip from Resources.
        musicGameplay  = Resources.Load<AudioClip>("Audio/music_2");
        // Load the jump sound effect from Resources.
        sfxJump        = Resources.Load<AudioClip>("Audio/jump_sound_effect");
        // Load the splash sound effect from Resources.
        sfxSplash      = Resources.Load<AudioClip>("Audio/water-splash_effect");
        // Load the yellow-balloon collect sound effect from Resources.
        sfxYellow      = Resources.Load<AudioClip>("Audio/Yellow_Collect_Effect");
        // Load the black-balloon collect sound effect from Resources.
        sfxBlack       = Resources.Load<AudioClip>("Audio/Black Balloon_Collect");
        // Load the game-over sound effect from Resources.
        sfxGameOver    = Resources.Load<AudioClip>("Audio/Character_Died_Game_Over");
    }

    private void ApplyMusicForScene(Scene scene)
    {
        // The menu is whichever scene sits at build index 0.
        // Choose the menu track for build index 0, otherwise the gameplay track.
        AudioClip target = scene.buildIndex == MainMenuBuildIndex ? musicMenu : musicGameplay;

        // Skip when the requested track is already playing.
        if (target == null || (musicSource.clip == target && musicSource.isPlaying))
        {
            return;
        }

        // Assign the target clip to the music source.
        musicSource.clip = target;
        // Loop the music indefinitely.
        musicSource.loop = true;
        // Start playing the new music track.
        musicSource.Play();
    }

    // Play the jump sound effect.
    public void PlayJump()        => PlayOneShot(sfxJump);
    // Play the splash sound effect.
    public void PlaySplash()      => PlayOneShot(sfxSplash);
    // Play the yellow-balloon collect sound effect.
    public void PlayYellow()      => PlayOneShot(sfxYellow);
    // Play the black-balloon collect sound effect.
    public void PlayBlack()       => PlayOneShot(sfxBlack);
    // Play the game-over sound effect.
    public void PlayGameOver()    => PlayOneShot(sfxGameOver);

    // Halt the looping background music. Used when the game ends.
    public void StopMusic()
    {
        // Only call Stop when the music source still exists.
        if (musicSource != null)
        {
            // Stop the looping music immediately.
            musicSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        // Skip when either the clip or the source is missing.
        if (clip == null || sfxSource == null)
        {
            return;
        }

        // Play the clip exactly once through the SFX source.
        sfxSource.PlayOneShot(clip);
    }
}
