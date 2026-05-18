using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ImportedScenes.PurlySnowman_A04_20260427_Copy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PurlyController : MonoBehaviour
    {
        // Store how quickly Purly moves left and right.
        [SerializeField] private float moveSpeed = 6f;

        // Store the upward velocity applied when Purly jumps.
        [SerializeField] private float jumpForce = 15f;

        // Store the transform used for ground detection.
        [SerializeField] private Transform groundCheck;

        // Store the radius used for ground detection.
        [SerializeField] private float groundCheckRadius = 0.25f;

        // Store which layers count as ground.
        [SerializeField] private LayerMask groundLayer;

        // Store the short post-edge grace period for jumping.
        [SerializeField] private float coyoteTime = 0.12f;

        // Keep this field for Inspector compatibility with the current scene setup.
        [SerializeField] private float jumpBuffer = 0.12f;

        // Store the Y value below which Purly dies.
        [SerializeField] private float deathY = -10f;

        // Store the continuous spin speed used by Q and E.
        [SerializeField] private float spinSpeed = 220f;

        // Store the duration of the automatic R-key spin.
        [SerializeField] private float autoSpinDuration = 0.6f;

        // Cache Purly's rigidbody for movement and jumping.
        private Rigidbody2D rb;

        // Cache Purly's animator for clip playback.
        private Animator anim;

        // Store the current horizontal input value.
        private float horizontalInput;

        // Store whether Purly is currently grounded.
        private bool isGrounded;

        // Store whether Purly has already died.
        private bool isDead;

        // Store the remaining coyote-time window.
        private float coyoteTimer;

        // Store whether the automatic spin coroutine is currently running.
        private bool isSpinning;

        // Store the currently playing animator-state hash.
        private int currentStateHash;

        // Tracks the previous-frame grounded value so we can detect a landing transition.
        private bool wasGroundedLastFrame;

        // Store the idle state name.
        private const string IdleStateName = "Idle";

        // Store the right-walk state name.
        private const string WalkRightStateName = "WalkRight";

        // Store the left-walk state name.
        private const string WalkLeftStateName = "WalkLeft";

        // Store the jump state name.
        private const string JumpStateName = "Jump";

        private void Awake()
        {
            // Cache the rigidbody component on this object.
            rb = GetComponent<Rigidbody2D>();

            // Cache the animator component on this object.
            anim = GetComponent<Animator>();

            // Keep the animator enabled so the new clips can play.
            anim.enabled = true;

            // Prevent 2D physics from rotating Purly on the Z axis.
            rb.freezeRotation = true;

            // Assume Purly starts standing so the first FixedUpdate does not splash on spawn.
            wasGroundedLastFrame = true;

            // Start on the idle state.
            PlayState(IdleStateName, true);
        }

        private void Update()
        {
            // Stop all input processing after death.
            if (isDead)
            {
                return;
            }

            // Read the current keyboard device.
            Keyboard keyboard = Keyboard.current;

            // Stop when no keyboard is available.
            if (keyboard == null)
            {
                return;
            }

            // Convert left/right keys into a single horizontal input value.
            horizontalInput =
                (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);

            // Count down the coyote-time timer every frame.
            coyoteTimer -= Time.deltaTime;

            // Refresh coyote time while Purly is grounded.
            if (isGrounded)
            {
                coyoteTimer = coyoteTime;
            }

            // Jump immediately when Space is pressed inside the coyote-time window.
            if (keyboard.spaceKey.wasPressedThisFrame && coyoteTimer > 0f)
            {
                coyoteTimer = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                PlayState(JumpStateName, true);
                GameAudio.Instance?.PlayJump();
            }

            // Handle Q and E hold spins only while no auto-spin is in progress.
            if (!isSpinning)
            {
                // Build one combined spin direction from the current keys.
                float spinInput =
                    (keyboard.eKey.isPressed ? 1f : 0f) -
                    (keyboard.qKey.isPressed ? 1f : 0f);

                // Apply continuous spin when Q or E is held.
                if (Mathf.Abs(spinInput) > 0.01f)
                {
                    transform.Rotate(0f, spinInput * spinSpeed * Time.deltaTime, 0f);
                }

                // Start the automatic full spin when R is pressed.
                if (Mathf.Abs(spinInput) <= 0.01f && keyboard.rKey.wasPressedThisFrame)
                {
                    StartCoroutine(SpinRoutine(1f));
                }
            }

            // Kill Purly when he falls below the death line.
            if (transform.position.y < deathY)
            {
                Die();
            }

            // Refresh the current animation state.
            UpdateAnimationState();
        }

        private void FixedUpdate()
        {
            // Stop movement updates after death.
            if (isDead)
            {
                return;
            }

            // Check whether Purly is standing on ground.
            isGrounded = CheckGrounded();

            // Apply horizontal movement while keeping the current vertical velocity.
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

            // Fire splash + sound when Purly transitions from airborne to grounded.
            if (isGrounded && !wasGroundedLastFrame)
            {
                GameAudio.Instance?.PlaySplash();
                SplashEffect.Spawn(groundCheck != null ? groundCheck.position : transform.position);
            }

            wasGroundedLastFrame = isGrounded;
        }

        private bool CheckGrounded()
        {
            // Use the feet marker when it exists.
            Vector2 checkPosition = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position + Vector2.down * 0.5f;

            // Look for one overlapping ground collider.
            Collider2D hit = Physics2D.OverlapCircle(
                checkPosition,
                Mathf.Max(groundCheckRadius, 0.3f),
                groundLayer
            );

            // Return true when the overlap belongs to something outside Purly's own hierarchy.
            if (hit != null && hit.transform.root != transform)
            {
                return true;
            }

            // Fall back to a low vertical-speed check for imperfect ground setups.
            return rb.linearVelocity.y > -0.5f && rb.linearVelocity.y < 0.5f;
        }

        private void UpdateAnimationState()
        {
            // Keep the jump clip active while Purly is airborne.
            if (!isGrounded)
            {
                PlayState(JumpStateName);
                return;
            }

            // Play the right-walk clip while moving right.
            if (horizontalInput > 0.01f)
            {
                PlayState(WalkRightStateName);
                return;
            }

            // Play the left-walk clip while moving left.
            if (horizontalInput < -0.01f)
            {
                PlayState(WalkLeftStateName);
                return;
            }

            // Return to idle when Purly is standing still.
            PlayState(IdleStateName);
        }

        private void PlayState(string stateName, bool restart = false)
        {
            // Convert the requested state name into a short-name Animator hash.
            int stateHash = Animator.StringToHash(stateName);

            // Skip replaying the same state unless a restart was requested.
            if (!restart && currentStateHash == stateHash)
            {
                return;
            }

            // Stop when the requested state does not exist on layer 0.
            if (!anim.HasState(0, stateHash))
            {
                return;
            }

            // Play the requested state from the beginning.
            anim.Play(stateHash, 0, 0f);

            // Remember which state is now active.
            currentStateHash = stateHash;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Kill Purly when he touches a waterfall trigger.
            if (other.CompareTag("Waterfall"))
            {
                Die();
            }

            // Kill Purly when he touches a death-zone trigger.
            if (other.CompareTag("DeathZone"))
            {
                Die();
            }
        }

        private IEnumerator SpinRoutine(float direction)
        {
            // Lock manual spin input while the auto-spin runs.
            isSpinning = true;

            // Store how much time has passed in the spin.
            float elapsed = 0f;

            // Store how many degrees were already applied.
            float appliedAngle = 0f;

            // Continue until the full auto-spin duration has elapsed.
            while (elapsed < autoSpinDuration)
            {
                // Advance the elapsed time.
                elapsed += Time.deltaTime;

                // Convert elapsed time into a 0..1 progress value.
                float progress = Mathf.Clamp01(elapsed / autoSpinDuration);

                // Compute the total angle that should be reached at this progress value.
                float targetAngle = direction * 360f * progress;

                // Compute only the extra angle needed this frame.
                float deltaAngle = targetAngle - appliedAngle;

                // Apply that extra angle around the Y axis.
                transform.Rotate(0f, deltaAngle, 0f);

                // Remember the total angle already applied.
                appliedAngle = targetAngle;

                // Wait until the next frame.
                yield return null;
            }

            // Reset Purly's local rotation after the spin completes.
            transform.localEulerAngles = Vector3.zero;

            // Unlock manual spin input.
            isSpinning = false;
        }

        public void Die()
        {
            // Ignore duplicate death handling.
            if (isDead)
            {
                return;
            }

            // Mark Purly as dead.
            isDead = true;

            // Stop all current movement.
            rb.linearVelocity = Vector2.zero;

            // Stop the background gameplay music when the game ends.
            GameAudio.Instance?.StopMusic();

            // Play the game-over sound effect.
            GameAudio.Instance?.PlayGameOver();

            // Notify the active imported-scene game manager.
            GameManager.Instance?.OnPlayerDied();
        }

        private void OnDrawGizmosSelected()
        {
            // Stop if there is no ground-check transform to preview.
            if (groundCheck == null)
            {
                return;
            }

            // Draw the ground-check circle in red.
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
