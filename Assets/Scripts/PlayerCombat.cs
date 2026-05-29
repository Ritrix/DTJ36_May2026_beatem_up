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
        currentAttackData = attack;

        isAttacking = true;
        hitboxActive = false;
        canCancel = false;

        attackTimer = 0f;
        comboTimer = comboResetTime;

        usedAttacksThisCombo.Add(attack.name);

        Debug.Log($"Started attack: {attack.name}");

        // Later:
        // animator.Play(attack.animationName);
    }

    private void HandleAttackTimer()
    {
        if (!isAttacking || currentAttackData == null) return;

        attackTimer += Time.deltaTime;

        if (!hitboxActive && attackTimer >= currentAttackData.hitboxStartTime)
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
        hitboxActive = true;
        canCancel = true;

        Debug.Log("Hitbox active");

        CheckHits();
    }

    private void DeactivateHitbox()
    {
        hitboxActive = false;

        Debug.Log("Hitbox inactive");
    }

    private void EndAttack()
    {
        isAttacking = false;
        hitboxActive = false;
        canCancel = false;

        Debug.Log("Attack ended");
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
        usedAttacksThisCombo.Clear();
        comboTimer = 0f;

        Debug.Log("Combo reset");
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
        float facingMultiplier = transform.localScale.x >= 0 ? 1f : -1f;

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

        foreach (Collider2D hit in hits)
        {
            Vector2 hitPosition = hit.ClosestPoint(hitboxCenter);

            Debug.Log(
                $"Hit {hit.name} with {currentAttackData.name} " +
                $"for {currentAttackData.damage} damage at {hitPosition}"
            );

            // Later:
            // EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            // enemy.TakeHit(currentAttackData.damage, hitPosition);
        }
    }

    public void SetHurt(float duration)
    {
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