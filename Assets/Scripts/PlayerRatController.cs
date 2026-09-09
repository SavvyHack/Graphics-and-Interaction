using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player Rat Controller — Project R.A.T.
/// Handles horizontal movement, jumping, and the 2.5D plane restriction
/// (X = movement, Y = jump/fall, Z = locked).
///
/// Attach to the PlayerRat root object, which should be structured as:
///   PlayerRat (CharacterController + this script)
///   ├── Model        (visual mesh, child transform — flipped/rotated to face direction)
///   └── GroundCheck  (empty transform positioned at the rat's feet)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerRatController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Empty child transform placed at the rat's feet, used for ground detection.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Child transform holding the rat's visual model. Used for facing direction.")]
    [SerializeField] private Transform model;
    [Tooltip("Layers considered 'ground' for the ground check sphere.")]
    [SerializeField] private LayerMask groundLayers;
    [Tooltip("Radius of the ground check sphere.")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 50f;
    [Tooltip("Degrees per second used when smoothly turning the model to face a new direction. Set very high for an instant flip.")]
    [SerializeField] private float turnSpeed = 720f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [Tooltip("Seconds after walking off a ledge where a jump is still allowed.")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Header("2.5D Plane Lock")]
    [Tooltip("The fixed Z position the rat is constrained to.")]
    [SerializeField] private float fixedZ = 0f;

    // --- internal state ---
    private CharacterController controller;
    private float currentSpeed;      // signed horizontal speed, - left / + right
    private float verticalVelocity;  // Y axis velocity (gravity/jump)
    private float coyoteTimer;
    private bool isGrounded;
    private bool facingRight = true;

    // Animator parameter hashes (set these parameter names up on the Animator Controller)
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimVerticalVelocity = Animator.StringToHash("VerticalVelocity");
    private static readonly int AnimJumpTrigger = Animator.StringToHash("Jump");

    private Animator animator;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (groundCheck == null)
            Debug.LogWarning($"{name}: GroundCheck not assigned on PlayerRatController.");
        if (model == null)
            Debug.LogWarning($"{name}: Model transform not assigned on PlayerRatController.");
    }

    private void Update()
    {
        CheckGrounded();
        HandleHorizontalInput();
        HandleJump();
        ApplyGravity();
        Move();
        LockToPlane();
        UpdateAnimator();
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }
        else
        {
            // Fallback if no GroundCheck transform is assigned.
            isGrounded = controller.isGrounded;
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (verticalVelocity < 0f)
                verticalVelocity = -2f; // small downward stick force, keeps controller grounded
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void HandleHorizontalInput()
    {
        float input = 0f;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input += 1f;
        }

        bool sprinting = keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        float targetTopSpeed = sprinting ? sprintSpeed : moveSpeed;
        float targetSpeed = input * targetTopSpeed;

        // Accelerate toward target speed, decelerate toward zero when no input.
        float rate = Mathf.Abs(input) > 0.01f ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // Update facing direction based on actual movement intent, not residual momentum.
        if (input > 0.01f && !facingRight) SetFacing(true);
        else if (input < -0.01f && facingRight) SetFacing(false);
    }

    private void SetFacing(bool right)
    {
        facingRight = right;
        // Rotate the model around Y only — never around X/Z, and never rotate the root
        // (root rotation could tilt movement off the locked plane).
        if (model != null)
        {
            Quaternion target = Quaternion.Euler(0f, right ? 90f : -90f, 0f);
            model.rotation = target;
            // If turnSpeed-based smoothing is preferred instead of an instant snap, replace the
            // line above with:
            // model.rotation = Quaternion.RotateTowards(model.rotation, target, turnSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool canJump = isGrounded || coyoteTimer > 0f;

        if (jumpPressed && canJump)
        {
            // v = sqrt(2 * h * -g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f; // consume coyote time so it can't double-jump off the same ledge
            animator?.SetTrigger(AnimJumpTrigger);
        }
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void Move()
    {
        Vector3 velocity = new Vector3(currentSpeed, verticalVelocity, 0f);
        controller.Move(velocity * Time.deltaTime);
    }

    private void LockToPlane()
    {
        // Hard constraint: gameplay must stay on a single Z plane regardless of any
        // drift introduced by collisions or external forces.
        Vector3 pos = transform.position;
        if (!Mathf.Approximately(pos.z, fixedZ))
        {
            pos.z = fixedZ;
            transform.position = pos;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat(AnimSpeed, Mathf.Abs(currentSpeed));
        animator.SetBool(AnimIsGrounded, isGrounded);
        animator.SetFloat(AnimVerticalVelocity, verticalVelocity);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
