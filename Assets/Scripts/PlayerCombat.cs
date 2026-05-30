using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static AttackTypes;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Timing")]
    [SerializeField] private AttackData[] attacks;
    [SerializeField] private float comboResetTime = 0.8f;
    [SerializeField] private AttackData debugAttackData;

    [Header("Input Buffer")]
    [SerializeField] private float directionBufferTime = 0.15f;

    private Vector2 bufferedDirection;
    private float directionBufferTimer;

    private AttackData currentAttackData;

    private Vector2 moveInput;

    private PlayerMovement movement;

    private bool isAttacking;
    private bool hitboxActive;
    private bool canCancel;
    private bool isHurt;
    private int comboHitCount;
    private bool hitboxHasActivated;

    [Header("Debug")]
    [SerializeField] private bool drawHitboxes = true;

    private Vector2 lastHitboxCenter;
    private Vector2 lastHitboxSize;
    private float hitboxDrawTimer;

    private float attackTimer;
    private float comboTimer; 

    private AttackId currentAttack;
    private readonly HashSet<string> usedAttacksThisCombo = new();

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }



    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;

        if (input.sqrMagnitude > 0.1f)
        {
            bufferedDirection = input;
            directionBufferTimer = directionBufferTime;
        }
    }

    private AttackData GetAttackData(AttackStrength strength, AttackDirection direction)
    {
        foreach (AttackData attack in attacks)
        {
            if (attack.strength == strength && attack.direction == direction)
            {
                return attack;
            }
        }

        Debug.LogError($"Missing AttackData for {strength}_{direction}");
        return null;
    }

    private void Update()
    {
        HandleDirectionBuffer();
        HandleAttackTimer();
        HandleComboTimer();

        if (hitboxDrawTimer > 0f)
        {
            hitboxDrawTimer -= Time.deltaTime;
        }
    }

    private void HandleDirectionBuffer()
    {
        if (directionBufferTimer <= 0f) return;

        directionBufferTimer -= Time.deltaTime;

        if (directionBufferTimer <= 0f)
        {
            bufferedDirection = Vector2.zero;
        }
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        TryAttack(AttackStrength.Light);
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        TryAttack(AttackStrength.Heavy);
    }

    private void TryAttack(AttackStrength strength)
    {
        if (isHurt) return;

        Vector2 attackInput = directionBufferTimer > 0f
            ? bufferedDirection
            : moveInput;

        AttackDirection direction = GetAttackDirection(attackInput);

        AttackData nextAttack = GetAttackData(strength, direction);

        if (nextAttack == null) return;

        string attackKey = nextAttack.name;

        if (usedAttacksThisCombo.Contains(attackKey))
        {
            Debug.Log($"Cannot repeat attack in combo: {attackKey}");
            return;
        }

        if (isAttacking && !canCancel)
        {
            return;
        }

        StartAttack(nextAttack);
    }

    private void StartAttack(AttackData attack)
    {
        bool wasInterrupted = isAttacking;

        movement.SetMovementLocked(true);

        currentAttackData = attack;

        isAttacking = true;
        hitboxActive = false;
        hitboxHasActivated = false;
        canCancel = false;

        attackTimer = 0f;
        comboTimer = comboResetTime;

        string attackKey = $"{attack.strength}_{attack.direction}";
        usedAttacksThisCombo.Add(attackKey);
    }

    private void HandleAttackTimer()
    {
        if (!isAttacking || currentAttackData == null) return;

        attackTimer += Time.deltaTime;

        if (!hitboxHasActivated && attackTimer >= currentAttackData.hitboxStartTime)
        {
            ActivateHitbox();
        }

        if (hitboxActive && attackTimer >= currentAttackData.hitboxEndTime)
        {
            DeactivateHitbox();
        }

        if (attackTimer >= currentAttackData.attackDuration)
        {
            EndAttack();
        }
    }

    private void ActivateHitbox()
    {
        hitboxHasActivated = true;
        hitboxActive = true;
        canCancel = true;

        lastHitboxCenter = GetHitboxCenter(currentAttackData);
        lastHitboxSize = currentAttackData.hitboxSize;
        hitboxDrawTimer = currentAttackData.hitboxEndTime - currentAttackData.hitboxStartTime;

        CheckHits();
    }

    private void OnDrawGizmos()
    {
        if (!drawHitboxes) return;
        if (hitboxDrawTimer <= 0f) return;

        Gizmos.DrawWireCube(lastHitboxCenter, lastHitboxSize);
    }

    private void DeactivateHitbox()
    {
        hitboxActive = false;
    }

    private void EndAttack()
    {
        if (movement != null)
        {
            movement.SetMovementLocked(false);
        }
        isAttacking = false;
        hitboxActive = false;
        canCancel = false;
    }

    private void HandleComboTimer()
    {
        if (usedAttacksThisCombo.Count == 0) return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f && !isAttacking)
        {
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        Debug.Log($"Combo reset. Combo Hit Count: {comboHitCount}");

        usedAttacksThisCombo.Clear();
        comboTimer = 0f;
        comboHitCount = 0;

        
    }

    private AttackDirection GetAttackDirection(Vector2 input)
    {
        if (input.y > 0.5f)
            return AttackDirection.Up;

        if (input.y < -0.5f)
            return AttackDirection.Down;

        if (Mathf.Abs(input.x) > 0.5f)
            return AttackDirection.Side;

        return AttackDirection.Neutral;
    }

    private Vector2 GetHitboxCenter(AttackData attack)
    {
        float facingMultiplier = movement.FacingDirection();

        Vector2 offset = attack.hitboxOffset;
        offset.x *= facingMultiplier;

        return (Vector2)transform.position + offset;
    }

    private void CheckHits()
    {
        if (currentAttackData == null) return;

        Vector2 hitboxCenter = GetHitboxCenter(currentAttackData);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitboxCenter,
            currentAttackData.hitboxSize,
            0f,
            enemyLayer
        );

        if (hits.Length == 0)
        {
            return;
        }

        foreach (Collider2D hit in hits)
        {
            Vector2 hitPosition = hit.ClosestPoint(hitboxCenter);

            comboHitCount++;

            Camera.main.GetComponent<SimpleBeatEmUpCamera>().Shake(0.4f);

            Debug.Log(
                $"Hit detected!\n" +
                $"Attack: {currentAttackData.name}\n" +
                $"Damage: {currentAttackData.damage}\n" +
                $"Hitbox Center: {hitboxCenter}\n" +
                $"Hit Position: {hitPosition}\n" +
                $"Combo Hits: {comboHitCount}"
            );

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            int direction = movement.FacingDirection();

            if (enemy != null)
            {
                enemy.TakeHit(currentAttackData, hitPosition, comboHitCount, direction);
            }
        }
    }

    public void SetHurt(float duration)
    {
        if (movement != null)
        {
            movement.SetMovementLocked(false);
        }
        isHurt = true;
        isAttacking = false;
        hitboxActive = false;
        canCancel = false;

        Invoke(nameof(RecoverFromHurt), duration);
    }

    private void RecoverFromHurt()
    {
        isHurt = false;
    }

    private void OnDrawGizmosSelected()
    {
        AttackData attackToDraw = currentAttackData != null
            ? currentAttackData
            : debugAttackData;

        if (attackToDraw == null) return;

        Vector2 hitboxCenter = GetHitboxCenter(attackToDraw);

        Gizmos.DrawWireCube(hitboxCenter, attackToDraw.hitboxSize);
    }
}