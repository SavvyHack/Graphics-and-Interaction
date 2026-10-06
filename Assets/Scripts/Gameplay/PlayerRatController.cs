using UnityEngine;
using UnityEngine.InputSystem;


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

    [Header("Augments (levels 3-7)")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.22f;
    [SerializeField] private float wallSlideSpeed = 2.5f;
    [SerializeField] private float wallKickSpeed = 8f;
    [SerializeField] private float glideFallSpeed = 1.6f;
    [SerializeField] private float updraftSpeed = 7f;
    [SerializeField] private float poundSpeed = 24f;

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

    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimVerticalVelocity = Animator.StringToHash("VerticalVelocity");
    private static readonly int AnimJumpTrigger = Animator.StringToHash("Jump");

    private Animator animator;
    private RatPowerups powerups;
    private bool usedAirJump;
    private float jumpBuffer;
    private float horizontalInput;
    private float dashTimer, wallTimer, wallLock;
    private int dashDirection, wallDirection;
    private bool dashUsed, pounding, passThroughHatch;

    public bool IsDashing => dashTimer > 0f;
    public bool IsPounding => pounding;
    public bool IsGliding { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        powerups = GetComponent<RatPowerups>();

        if (groundCheck == null)
            Debug.LogWarning($"{name}: GroundCheck not assigned on PlayerRatController.");
        if (model == null)
            Debug.LogWarning($"{name}: Model transform not assigned on PlayerRatController.");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (Time.deltaTime <= 0f) return;
        CheckGrounded();
        HandleHorizontalInput();
        HandleAugmentInput();
        HandleJump();
        ApplyGravity();
        Move();
        LockToPlane();
        UpdateAnimator();
    }

    private void CheckGrounded()
    {
        // A smashed hatch leaves no floor: keep slamming down to the next one.
        if (passThroughHatch)
        {
            passThroughHatch = false;
            isGrounded = false;
            return;
        }

        // Ignore the previous move's ground contact while the rat is rising.
        if (verticalVelocity > 0f)
        {
            isGrounded = false;
            coyoteTimer = 0f;
            return;
        }

        // Use actual CharacterController contact, not nearby foot-sphere overlaps.
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            usedAirJump = false;
            if (dashTimer <= 0f) dashUsed = false;
            if (pounding) { pounding = false; AudioManager.Instance?.PlayJump(); }
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
        horizontalInput = input;

        // Dash, ground pound and a wall kick briefly own horizontal motion.
        if (dashTimer > 0f) { dashTimer -= Time.deltaTime; currentSpeed = dashDirection * dashSpeed; return; }
        if (pounding) { currentSpeed = 0f; return; }
        if (wallLock > 0f) { wallLock -= Time.deltaTime; return; }

        bool sprinting = keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        float targetTopSpeed = sprinting ? sprintSpeed : moveSpeed;
        float targetSpeed = input * targetTopSpeed * (powerups != null ? powerups.SpeedMultiplier : 1f);

        // Accelerate toward target speed, decelerate toward zero when no input.
        float rate = Mathf.Abs(input) > 0.01f ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // Update facing direction based on movement intent, not residual momentum.
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

    // Dash (Q) and ground pound (S / Down) are pressed once; both need their augment active.
    private void HandleAugmentInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || powerups == null) return;
        if (keyboard.qKey.wasPressedThisFrame && powerups.Has(RatAugment.Dash) && !dashUsed && dashTimer <= 0f && !pounding)
        {
            dashTimer = dashTime;
            dashDirection = facingRight ? 1 : -1;
            dashUsed = true;
            AudioManager.Instance?.PlayJump();
        }
        if ((keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) &&
            powerups.Has(RatAugment.GroundPound) && !isGrounded && !pounding)
        {
            pounding = true;
            dashTimer = 0f;
            currentSpeed = 0f;
            verticalVelocity = -poundSpeed;
        }
    }

    // Augment jump logic adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Scripts/Gameplay/PlayerRatController.cs
    // Local crate pushing and platform carry are retained.
    private void HandleJump()
    {
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        jumpBuffer = jumpPressed ? .12f : Mathf.Max(0, jumpBuffer - Time.deltaTime);
        if (pounding) return;
        bool canJump = isGrounded || coyoteTimer > 0f;
        bool wallJump = !canJump && wallTimer > 0f && powerups != null && powerups.Has(RatAugment.WallJump);
        bool airJump = !canJump && !wallJump && !usedAirJump && powerups != null && powerups.Has(RatAugment.DoubleJump);

        if (jumpBuffer > 0 && (canJump || wallJump || airJump))
        {
            // v = sqrt(2 * h * -g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBuffer = 0;
            dashTimer = 0f;
            if (wallJump)
            {
                // Kick away from the wall; input is ignored briefly so the kick always clears it.
                currentSpeed = -wallDirection * wallKickSpeed;
                wallLock = 0.18f;
                wallTimer = 0f;
                dashUsed = false;
                SetFacing(wallDirection < 0);
            }
            else if (!canJump) usedAirJump = true;
            coyoteTimer = 0f; // consume coyote time so it can't double-jump off the same ledge
            animator?.SetTrigger(AnimJumpTrigger);
            AudioManager.Instance?.PlayJump();
        }
        if (!isGrounded && powerups != null && Keyboard.current != null && Keyboard.current.spaceKey.isPressed && powerups.UseJet(Time.deltaTime))
            verticalVelocity = Mathf.Max(verticalVelocity, Mathf.MoveTowards(verticalVelocity, 6.5f, 65f * Time.deltaTime));
    }

    private void ApplyGravity()
    {
        IsGliding = false;
        if (dashTimer > 0f) { verticalVelocity = 0f; return; } // A dash flies level.
        verticalVelocity += gravity * Time.deltaTime;
        if (pounding) { verticalVelocity = Mathf.Min(verticalVelocity, -poundSpeed); return; }
        if (isGrounded || powerups == null) return;

        bool held = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool jetting = held && powerups.Has(RatAugment.Jetpack) && powerups.JetFuel > 0f;
        if (held && !jetting && powerups.Has(RatAugment.Glide))
        {
            bool lifted = Updraft.Lifts(transform.position + controller.center);
            if (lifted) verticalVelocity = Mathf.MoveTowards(verticalVelocity, updraftSpeed, 40f * Time.deltaTime);
            else verticalVelocity = Mathf.Max(verticalVelocity, -glideFallSpeed);
            IsGliding = lifted || verticalVelocity <= 0f;
        }
        // Holding towards a wall slows the fall so the rat can line up a wall kick.
        if (wallTimer > 0f && powerups.Has(RatAugment.WallJump) && horizontalInput * wallDirection > 0f)
            verticalVelocity = Mathf.Max(verticalVelocity, -wallSlideSpeed);
    }

    private void Move()
    {
        float horizontalSpeed = currentSpeed;
        // Probe before movement: crate and rat advance by the same resisted distance.
        if (isGrounded && Mathf.Abs(currentSpeed) > 0.01f)
        {
            Vector3 centre = transform.TransformPoint(controller.center);
            float cap = Mathf.Max(0f, controller.height * 0.5f - controller.radius);
            if (Physics.CapsuleCast(centre + Vector3.up * cap, centre - Vector3.up * cap,
                controller.radius * 0.95f, Vector3.right * Mathf.Sign(currentSpeed),
                out RaycastHit contact, Mathf.Abs(currentSpeed) * Time.deltaTime + 0.08f,
                ~0, QueryTriggerInteraction.Ignore))
            {
                PushBlock block = contact.collider.GetComponentInParent<PushBlock>();
                if (block != null)
                {
                    float request = currentSpeed * block.speedMultiplier * Time.deltaTime;
                    float pushed = block.Push(request, controller);
                    horizontalSpeed = pushed / Mathf.Max(Time.deltaTime, 0.0001f);
                }
            }
        }
        Vector3 velocity = new Vector3(horizontalSpeed, verticalVelocity, 0f);
        Vector3 carry = Vector3.zero;
        if (verticalVelocity <= 0f && Physics.SphereCast(
            transform.position + controller.center, controller.radius * 0.8f, Vector3.down,
            out RaycastHit support, controller.height * 0.5f - controller.radius * 0.8f + 0.3f,
            groundLayers, QueryTriggerInteraction.Ignore))
        {
            TrialMovingPlatform platform = support.collider.GetComponentInParent<TrialMovingPlatform>();
            if (platform != null) carry = platform.Delta;
        }
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime + carry);
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
        // Remember a wall touched in mid-air for a short grace period (wall jump).
        if ((flags & CollisionFlags.Sides) != 0 && !isGrounded && Mathf.Abs(horizontalSpeed) > 0.01f)
        {
            wallDirection = horizontalSpeed > 0f ? 1 : -1;
            wallTimer = 0.12f;
        }
        else wallTimer = Mathf.Max(0f, wallTimer - Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!pounding || hit.normal.y < 0.5f) return;
        BreakableHatch hatch = hit.collider.GetComponentInParent<BreakableHatch>();
        if (hatch != null && hatch.Break()) passThroughHatch = true;
    }

    public void Respawn(Vector3 position)
    {
        controller.enabled = false;
        transform.position = new Vector3(position.x, position.y, fixedZ);
        ResetMotion();
        controller.enabled = true;
    }

    /// <summary>Clear momentum and stale jump state when the life manager activates a rat.</summary>
    public void ResetMotion()
    {
        currentSpeed = verticalVelocity = coyoteTimer = jumpBuffer = 0f;
        dashTimer = wallTimer = wallLock = 0f;
        usedAirJump = dashUsed = pounding = passThroughHatch = false;
        IsGliding = false;
        isGrounded = false;
        animator?.ResetTrigger(AnimJumpTrigger);
        UpdateAnimator();
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
