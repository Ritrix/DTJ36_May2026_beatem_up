using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerCombat combat;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private SpriteRenderer spriteRenderer;

    public bool FacingRight { get; private set; } = true;
    public bool MovementLocked { get; private set; }

    public void SetMovementLocked(bool locked)
    {
        MovementLocked = locked;

        if (locked)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (MovementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * GetMoveSpeed();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (combat != null)
        {
            combat.SetMoveInput(moveInput);
        }

        // Flip the sprite based on movement direction
        if (moveInput.x > 0.1f)
        {
            FacingRight = true;
            spriteRenderer.flipX = false; // Facing right
        }
        else if (moveInput.x < -0.1f)
        {
            FacingRight = false;
            spriteRenderer.flipX = true; // Facing left
        }
    }

    public int FacingDirection()
    {
        return FacingRight ? 1 : -1;
    }

    private float GetMoveSpeed()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.ModifyMoveSpeed(moveSpeed)
            : moveSpeed;
    }
}
