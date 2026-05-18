using UnityEngine;

// Procedurally configured one-shot splash particle system.
// Attach to a player-owned child or call Play(position) on a runtime-spawned instance.
[RequireComponent(typeof(ParticleSystem))]
public class SplashEffect : MonoBehaviour
{
    private ParticleSystem ps;

    // Spawn a fresh splash at the given world position and play it once.
    public static void Spawn(Vector3 worldPosition)
    {
        GameObject host = new("SplashEffect");
        host.transform.position = worldPosition;
        host.AddComponent<ParticleSystem>();
        SplashEffect splash = host.AddComponent<SplashEffect>();
        splash.Play();
        Destroy(host, 1.2f);
    }

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        Configure();
    }

    public void Play()
    {
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
            Configure();
        }

        ps.Play();
    }

    private void Configure()
    {
        // Halt any auto-play before mutating duration — Unity rejects edits on a playing system.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.35f;
        main.gravityModifier = 1.5f;
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.8f, 0.9f, 1f, 1f);

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 25),
        });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.05f;
        shape.position = Vector3.zero;
        shape.rotation = new Vector3(-90f, 0f, 0f); // emit upward

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 40;

        Sprite splashSprite = Resources.Load<Sprite>("Sprites/water_splash");
        Texture2D tex = splashSprite != null ? splashSprite.texture : Resources.Load<Texture2D>("Sprites/water_splash");
        Material mat = new(Shader.Find("Sprites/Default"));
        if (tex != null)
        {
            mat.mainTexture = tex;
        }
        renderer.material = mat;
    }
}
