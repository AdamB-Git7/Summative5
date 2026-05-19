// Bring in core Unity types like MonoBehaviour and ParticleSystem.
using UnityEngine;

// Procedurally configured one-shot splash particle system.
// Attach to a player-owned child or call Play(position) on a runtime-spawned instance.
[RequireComponent(typeof(ParticleSystem))]
public class SplashEffect : MonoBehaviour
{
    // Cache the particle system component on this object.
    private ParticleSystem ps;

    // Spawn a fresh splash at the given world position and play it once.
    public static void Spawn(Vector3 worldPosition)
    {
        // Create a temporary host GameObject for the splash.
        GameObject host = new("SplashEffect");
        // Move the host to the requested world position.
        host.transform.position = worldPosition;
        // Add the ParticleSystem component that the splash requires.
        host.AddComponent<ParticleSystem>();
        // Add the SplashEffect behavior so the system gets configured.
        SplashEffect splash = host.AddComponent<SplashEffect>();
        // Trigger the splash to start playing.
        splash.Play();
        // Auto-destroy the splash host after its particles finish.
        Destroy(host, 1.2f);
    }

    private void Awake()
    {
        // Cache the particle system component.
        ps = GetComponent<ParticleSystem>();
        // Configure the system with the splash settings.
        Configure();
    }

    public void Play()
    {
        // Re-acquire and re-configure the particle system if it has not been cached yet.
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
            Configure();
        }

        // Start emitting the splash particles.
        ps.Play();
    }

    private void Configure()
    {
        // Halt any auto-play before mutating duration — Unity rejects edits on a playing system.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Grab the main module so duration and color can be set.
        ParticleSystem.MainModule main = ps.main;
        // Disable auto-play so the splash only fires on demand.
        main.playOnAwake = false;
        // Set the total burst duration.
        main.duration = 0.4f;
        // Do not loop; the splash is one-shot.
        main.loop = false;
        // Each particle lives for 0.4 seconds.
        main.startLifetime = 0.4f;
        // Each particle starts at 3 units per second.
        main.startSpeed = 3f;
        // Each particle starts at 0.35 world units in size.
        main.startSize = 0.35f;
        // Apply gravity so droplets arc back down.
        main.gravityModifier = 1.5f;
        // Cap the total particle count.
        main.maxParticles = 60;
        // Simulate particles in world space so they stay put as the parent moves.
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // Tint the splash a soft icy blue.
        main.startColor = new Color(0.8f, 0.9f, 1f, 1f);

        // Grab the emission module to set the burst.
        ParticleSystem.EmissionModule emission = ps.emission;
        // No continuous emission rate; the burst alone drives the splash.
        emission.rateOverTime = 0f;
        // Emit 25 particles at time zero in a single burst.
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 25),
        });

        // Grab the shape module to control the emission cone.
        ParticleSystem.ShapeModule shape = ps.shape;
        // Use a cone shape for the splash.
        shape.shapeType = ParticleSystemShapeType.Cone;
        // Set the cone's spread angle.
        shape.angle = 35f;
        // Set a small base radius for the cone.
        shape.radius = 0.05f;
        // Place the cone origin at the host's pivot.
        shape.position = Vector3.zero;
        shape.rotation = new Vector3(-90f, 0f, 0f); // emit upward

        // Grab the size-over-lifetime module to shrink particles as they age.
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
        // Enable the module.
        sizeOverLifetime.enabled = true;
        // Build the size curve in code.
        AnimationCurve curve = new();
        // Start at full size at birth.
        curve.AddKey(0f, 1f);
        // Shrink to zero by the end of life.
        curve.AddKey(1f, 0f);
        // Apply the curve to the size-over-lifetime module.
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Grab the renderer so the sprite and sorting can be set.
        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        // Render particles as camera-facing billboards.
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // Draw splashes above ground but below UI.
        renderer.sortingOrder = 40;

        // Try to load the splash sprite from Resources.
        Sprite splashSprite = Resources.Load<Sprite>("Sprites/water_splash");
        // Fall back to loading a Texture2D when the Sprite asset is missing.
        Texture2D tex = splashSprite != null ? splashSprite.texture : Resources.Load<Texture2D>("Sprites/water_splash");
        // Build a runtime material using the default sprite shader.
        Material mat = new(Shader.Find("Sprites/Default"));
        // Apply the splash texture if one was loaded.
        if (tex != null)
        {
            mat.mainTexture = tex;
        }
        // Apply the material to the particle renderer.
        renderer.material = mat;
    }
}
