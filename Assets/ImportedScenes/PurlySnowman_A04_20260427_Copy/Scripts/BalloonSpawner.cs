using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImportedScenes.PurlySnowman_A04_20260427_Copy
{
    public class BalloonSpawner : MonoBehaviour
    {
        // Store the prefab that will be cloned for each balloon.
        [SerializeField] private GameObject balloonPrefab;

        // Store fallback spawn points for scenes without live markers.
        [SerializeField] private Transform[] spawnPoints;

        // Store the shortest allowed delay between spawn attempts.
        [SerializeField] private float minSpawnDelay = 0.6f;

        // Store the longest allowed delay between spawn attempts.
        [SerializeField] private float maxSpawnDelay = 1.6f;

        // Store the cap used to stop too many balloons from existing at once.
        [SerializeField] private int maxActiveBalloons = 12;

        // Optional explicit sprite for black balloons (otherwise loaded from Resources).
        [SerializeField] private Sprite blackBalloonSprite;

        // Track all balloons that are currently alive.
        private readonly List<GameObject> alive = new();

        // Counts yellow spawns so we can drop a black after every 5.
        private int yellowSinceLastBlack;

        // Lazy-loaded black sprite when none was assigned in the Inspector.
        private Sprite cachedBlackSprite;

        private void Start()
        {
            // Stop immediately if the prefab was not assigned.
            if (balloonPrefab == null)
            {
                Debug.LogError("BalloonSpawner: balloonPrefab is not assigned.");
                return;
            }

            // Start the repeating spawn coroutine.
            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            // Keep spawning until this spawner is destroyed.
            while (true)
            {
                // Wait a random time before trying the next spawn.
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

                // Remove destroyed balloons from the tracking list.
                alive.RemoveAll(balloon => balloon == null);

                // Skip this spawn if we are already at the balloon cap.
                if (alive.Count >= maxActiveBalloons)
                {
                    continue;
                }

                // Find one valid spawn point.
                Transform point = PickSpawnPoint();

                // Skip this spawn if no point is available.
                if (point == null)
                {
                    continue;
                }

                // Add a small random offset so balloons do not overlap perfectly.
                Vector3 offset = new(
                    Random.Range(-0.2f, 0.2f),
                    Random.Range(0f, 0.3f),
                    0f
                );

                // Create the balloon and remember it in the alive list.
                GameObject instance = Instantiate(balloonPrefab, point.position + offset, Quaternion.identity);

                // Decide whether this should become a black balloon. 5 yellow then 1 black.
                if (yellowSinceLastBlack >= 5)
                {
                    Balloon balloon = instance.GetComponent<Balloon>();
                    if (balloon != null)
                    {
                        balloon.SetBlack(GetBlackSprite());
                    }
                    yellowSinceLastBlack = 0;
                }
                else
                {
                    yellowSinceLastBlack++;
                }

                alive.Add(instance);
            }
        }

        private Sprite GetBlackSprite()
        {
            // Prefer the Inspector-assigned sprite when present.
            if (blackBalloonSprite != null)
            {
                return blackBalloonSprite;
            }

            // Lazy-load the bundled Resources copy on first use.
            if (cachedBlackSprite == null)
            {
                cachedBlackSprite = Resources.Load<Sprite>("Sprites/Black Balloon");
            }

            return cachedBlackSprite;
        }

        private Transform PickSpawnPoint()
        {
            // Prefer live markers that currently exist in the scene.
            BalloonSpawnPoint[] livePoints = Object.FindObjectsByType<BalloonSpawnPoint>(FindObjectsSortMode.None);

            // Return one random live marker if any were found.
            if (livePoints.Length > 0)
            {
                return livePoints[Random.Range(0, livePoints.Length)].transform;
            }

            // Return one random fallback point if any were configured.
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                return spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            // Return nothing if there are no valid spawn locations.
            return null;
        }

        private void OnDrawGizmos()
        {
            // Stop if there are no fallback points to preview.
            if (spawnPoints == null)
            {
                return;
            }

            // Draw fallback spawn points in yellow.
            Gizmos.color = Color.yellow;

            // Visit each fallback point one by one.
            foreach (Transform point in spawnPoints)
            {
                // Skip empty array entries safely.
                if (point == null)
                {
                    continue;
                }

                // Draw a small sphere where a balloon could appear.
                Gizmos.DrawWireSphere(point.position, 0.25f);
            }
        }
    }
}
