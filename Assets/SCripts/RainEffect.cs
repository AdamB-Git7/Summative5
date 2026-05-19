// Bring in core Unity types like MonoBehaviour and ParticleSystem.
using UnityEngine;
// Bring in scene-management types used to react to scene loads.
using UnityEngine.SceneManagement;

// Spawns a procedurally configured rainfall particle system above the active main camera
// so every gameplay scene gets medium-frequency rain without any Inspector wiring.
[DisallowMultipleComponent]
public class RainEffect : MonoBehaviour
{
    // Build index of the menu scene; rain is skipped there.
    private const int MainMenuBuildIndex = 0;

    // Store the runtime-built rain particle system.
    private ParticleSystem rainSystem;
    // Store the camera the emitter follows.
    private Camera trackedCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapInitial()
    {
        // Make sure rain exists for the first scene that loads.
        EnsureForActiveScene();
    }

    private static void EnsureForActiveScene()
    {
        // Skip menu scenes; rain is gameplay-only.
        if (SceneManager.GetActiveScene().buildIndex == MainMenuBuildIndex)
        {
            return;
        }

        // Skip if a RainEffect already lives in the scene.
        if (FindAnyObjectByType<RainEffect>() != null)
        {
            return;
        }

        // Create a host GameObject to hold the rain.
        GameObject host = new("RainEffect");
        // Add the RainEffect component so the system gets built.
        host.AddComponent<RainEffect>();
    }

    private void Awake()
    {
        // React to future scene loads (the bootstrap only runs once per app start).
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid leaking the event handler.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-check whether the newly loaded scene needs a rain emitter.
        EnsureForActiveScene();
    }

    private void Start()
    {
        // Cache a reference to the main camera at startup.
        trackedCamera = Camera.main;
        // Build the rain particle system in code.
        BuildSystem();
    }

    private void LateUpdate()
    {
        // Re-acquire the camera if the previous one was destroyed.
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
        }

        // Keep the emitter perched above the camera as it moves.
        if (trackedCamera != null)
        {
            // Read the camera's current position.
            Vector3 camPos = trackedCamera.transform.position;
            // Sit the emitter eight units above the camera.
            transform.position = new Vector3(camPos.x, camPos.y + 8f, 0f);
        }
    }

    private void BuildSystem()
    {
        // Add a particle system component to this host.
        rainSystem = gameObject.AddComponent<ParticleSystem>();

        // Stop the auto-started system so duration changes are accepted.
        rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Grab the main module to configure global particle settings.
        ParticleSystem.MainModule main = rainSystem.main;
        // Disable auto-play so we can configure cleanly before playing.
        main.playOnAwake = false;
        // Use a 5-second loop interval.
        main.duration = 5f;
        // Loop forever.
        main.loop = true;
        // Each raindrop lives 1.6 seconds.
        main.startLifetime = 1.6f;
        main.startSpeed = 12f;       // medium speed
        // Each raindrop starts at 0.25 world units in size.
        main.startSize = 0.25f;
        // Tint the drops a soft cool blue.
        main.startColor = new Color(0.7f, 0.85f, 1f, 1f);
        // Apply normal gravity so drops fall.
        main.gravityModifier = 1f;
        // Allow up to 1500 active raindrops at once.
        main.maxParticles = 1500;
        // Simulate in world space so drops are not dragged by the host.
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Grab the emission module to control particle output rate.
        ParticleSystem.EmissionModule emission = rainSystem.emission;
        emission.rateOverTime = 180f; // medium frequency

        // Grab the shape module to configure the emitter volume.
        ParticleSystem.ShapeModule shape = rainSystem.shape;
        // Emit raindrops from a flat box high above the player.
        shape.shapeType = ParticleSystemShapeType.Box;
        // Stretch the box 40 units wide so rain covers the screen.
        shape.scale = new Vector3(40f, 0.1f, 1f);
        // Center the box on the emitter origin.
        shape.position = new Vector3(0f, 0f, 0f);
        // Keep the box axis-aligned.
        shape.rotation = new Vector3(0f, 0f, 0f);

        // Renderer setup uses an unlit additive material with the rain drop sprite.
        // Grab the particle renderer.
        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        // Stretch each drop along its velocity for a streak look.
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        // Set how long the stretch appears.
        renderer.lengthScale = 4f;
        // Boost the visual length based on velocity.
        renderer.velocityScale = 0.05f;
        // Draw rain on top of the gameplay scene.
        renderer.sortingOrder = 50;

        // Try to load the raindrop sprite asset.
        Sprite drop = Resources.Load<Sprite>("Sprites/rain_drop");
        // Fall back to a Texture2D when the Sprite was missing.
        Texture2D dropTex = drop != null ? drop.texture : Resources.Load<Texture2D>("Sprites/rain_drop");

        // Build a fresh material using the default sprite shader.
        Material rainMat = new(Shader.Find("Sprites/Default"));
        // Apply the raindrop texture if one was loaded.
        if (dropTex != null)
        {
            rainMat.mainTexture = dropTex;
        }
        // Attach the material to the renderer.
        renderer.material = rainMat;

        // Velocity over lifetime drives raindrops straight down.
        // All axes must use the same MinMaxCurve mode (two-constants here).
        ParticleSystem.VelocityOverLifetimeModule velocity = rainSystem.velocityOverLifetime;
        // Enable the velocity module.
        velocity.enabled = true;
        // Use world space so wind direction is consistent across the screen.
        velocity.space = ParticleSystemSimulationSpace.World;
        // Add a small random horizontal drift to break up perfect verticals.
        velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
        // Force a constant downward velocity so drops fall fast.
        velocity.y = new ParticleSystem.MinMaxCurve(-10f, -10f);
        // No depth motion.
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        // Configuration done — kick the rain off.
        rainSystem.Play();
    }
}
