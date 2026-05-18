using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    // Store how quickly the player moves left and right.
    [SerializeField] private float moveSpeed = 5f;

    // Store the upward velocity applied when the player jumps.
    [SerializeField] private float jumpForce = 10f;

    // Store the transform used for the ground overlap check.
    [SerializeField] private Transform groundCheck;

    // Store the radius used for the ground overlap check.
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Store which layers count as ground.
    [SerializeField] private LayerMask groundLayer;

    // Cache the player's Rigidbody2D once for reuse.
    private Rigidbody2D rb;

    // Store the current horizontal input value.
    private float moveInputX;

    // Store whether jump was pressed this frame.
    private bool jumpPressedThisFrame;

    // Tracks the previous-frame grounded value so we can detect a landing transition.
    private bool wasGroundedLastFrame;

    private void Awake()
    {
        // Cache the Rigidbody2D on this object.
        rb = GetComponent<Rigidbody2D>();

        // Assume Purly starts on the ground so the first FixedUpdate does not splash on spawn.
        wasGroundedLastFrame = true;
    }

    private void Update()
    {
        // Reset horizontal input before reading the keyboard.
        moveInputX = 0f;

        // Reset the jump flag before reading the keyboard.
        jumpPressedThisFrame = false;

        // Read the current keyboard device.
        Keyboard keyboard = Keyboard.current;

        // Stop if no keyboard is available.
        if (keyboard == null)
        {
            return;
        }

        // Move left when A or Left Arrow is held.
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            moveInputX = -1f;
        }

        // Move right when D or Right Arrow is held.
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            moveInputX = 1f;
        }

        // Store whether any supported jump key was pressed this frame.
        jumpPressedThisFrame =
            keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.wKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame;
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement while keeping the current vertical velocity.
        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);

        bool grounded = IsGrounded();

        // Apply the jump only when the key was pressed and the player is grounded.
        if (jumpPressedThisFrame && grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            GameAudio.Instance?.PlayJump();
        }

        // Fire splash + sound when Purly transitions from airborne to grounded.
        if (grounded && !wasGroundedLastFrame)
        {
            GameAudio.Instance?.PlaySplash();
            SplashEffect.Spawn(groundCheck != null ? groundCheck.position : transform.position);
        }

        wasGroundedLastFrame = grounded;

        // Clear the one-frame jump flag after the physics tick.
        jumpPressedThisFrame = false;
    }

    private bool IsGrounded()
    {
        // Return false when no ground-check transform was assigned.
        if (groundCheck == null)
        {
            return false;
        }

        // Return whether the ground-check circle overlaps any ground collider.
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void OnDrawGizmosSelected()
    {
        // Stop if there is no ground-check transform to preview.
        if (groundCheck == null)
        {
            return;
        }

        // Draw the ground-check circle in cyan.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
