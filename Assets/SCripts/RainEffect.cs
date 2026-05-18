using UnityEngine;
using UnityEngine.SceneManagement;

// Spawns a procedurally configured rainfall particle system above the active main camera
// so every gameplay scene gets medium-frequency rain without any Inspector wiring.
[DisallowMultipleComponent]
public class RainEffect : MonoBehaviour
{
    // Build index of the menu scene; rain is skipped there.
    private const int MainMenuBuildIndex = 0;

    private ParticleSystem rainSystem;
    private Camera trackedCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapInitial()
    {
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

        GameObject host = new("RainEffect");
        host.AddComponent<RainEffect>();
    }

    private void Awake()
    {
        // React to future scene loads (the bootstrap only runs once per app start).
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForActiveScene();
    }

    private void Start()
    {
        trackedCamera = Camera.main;
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
            Vector3 camPos = trackedCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y + 8f, 0f);
        }
    }

    private void BuildSystem()
    {
        rainSystem = gameObject.AddComponent<ParticleSystem>();

        // Stop the auto-started system so duration changes are accepted.
        rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = rainSystem.main;
        main.playOnAwake = false;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1.6f;
        main.startSpeed = 12f;       // medium speed
        main.startSize = 0.25f;
        main.startColor = new Color(0.7f, 0.85f, 1f, 1f);
        main.gravityModifier = 1f;
        main.maxParticles = 1500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = rainSystem.emission;
        emission.rateOverTime = 180f; // medium frequency

        ParticleSystem.ShapeModule shape = rainSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(40f, 0.1f, 1f);
        shape.position = new Vector3(0f, 0f, 0f);
        shape.rotation = new Vector3(0f, 0f, 0f);

        // Renderer setup uses an unlit additive material with the rain drop sprite.
        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 4f;
        renderer.velocityScale = 0.05f;
        renderer.sortingOrder = 50;

        Sprite drop = Resources.Load<Sprite>("Sprites/rain_drop");
        Texture2D dropTex = drop != null ? drop.texture : Resources.Load<Texture2D>("Sprites/rain_drop");

        Material rainMat = new(Shader.Find("Sprites/Default"));
        if (dropTex != null)
        {
            rainMat.mainTexture = dropTex;
        }
        renderer.material = rainMat;

        // Velocity over lifetime drives raindrops straight down.
        // All axes must use the same MinMaxCurve mode (two-constants here).
        ParticleSystem.VelocityOverLifetimeModule velocity = rainSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
        velocity.y = new ParticleSystem.MinMaxCurve(-10f, -10f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        // Configuration done — kick the rain off.
        rainSystem.Play();
    }
}
