using UnityEngine;

namespace ImportedScenes.PurlySnowman_A04_20260427_Copy
{
    public class Balloon : MonoBehaviour
    {
        // Score added when Purly pops this balloon. Black balloons override this to -3 at runtime.
        [SerializeField] private int scoreValue = 1;

        // Optional pop visual.
        [SerializeField] private GameObject popEffectPrefab;

        // Optional fallback pop sound if GameAudio is unavailable.
        [SerializeField] private AudioClip popSfx;

        // True when this balloon was reconfigured as a black balloon by the spawner.
        private bool isBlack;

        // Apply the black-balloon visual + score at spawn time.
        public void SetBlack(Sprite blackSprite)
        {
            isBlack = true;
            scoreValue = -3;

            // Swap the sprite when one is supplied.
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && blackSprite != null)
            {
                renderer.sprite = blackSprite;
                renderer.color = Color.white;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore anything that is not Purly.
            if (other.GetComponent<PurlyController>() == null)
            {
                return;
            }

            // Update the score.
            GameManager.Instance?.AddScore(scoreValue);

            // Play the right collect SFX through the global audio singleton.
            if (GameAudio.Instance != null)
            {
                if (isBlack)
                {
                    GameAudio.Instance.PlayBlack();
                }
                else
                {
                    GameAudio.Instance.PlayYellow();
                }
            }
            else if (popSfx != null)
            {
                AudioSource.PlayClipAtPoint(popSfx, transform.position);
            }

            // Spawn the pop effect if one was assigned in the Inspector.
            if (popEffectPrefab != null)
            {
                Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
            }

            // Remove the balloon after it has been popped.
            Destroy(gameObject);
        }
    }
}
