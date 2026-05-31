using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float movementStartDelay = 1f;

    [Header("Attack Position")]
    [SerializeField] private float desiredXDistance = 1f;
    [SerializeField] private float xTolerance = 0.15f;
    [SerializeField] private float yTolerance = 0.25f;
    [SerializeField] private float maxYVariation = 0.2f;
    [SerializeField] private float positionRefreshTime = 2f;

    [Header("Personal Space")]
    [SerializeField] private float minimumPersonalSpace = 0.8f;
    [SerializeField] private float retreatDistanceMin = 0.3f;
    [SerializeField] private float retreatDistanceMax = 0.8f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Attack Hitbox")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Vector2 attackHitboxSize = new Vector2(1f, 0.8f);
    [SerializeField] private Vector2 attackHitboxOffset = new Vector2(0.8f, 0f);
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Delay")]
    [SerializeField] private float attackDelayAfterArriving = 1f;

    [Header("Animation")]
    [SerializeField] private string idleAnimationState = "Enemy_Idle";
    [SerializeField] private string attackWindupAnimationState = "Enemy_AttackWindup";
    [SerializeField] private string attackHitAnimationState = "Enemy_AttackHit";
    [SerializeField] private float attackWindupTime = 0.35f;
    [SerializeField] private float attackHitFrameTime = 0.15f;

    private bool isAttacking;

    private Vector3 startingScale;

    private bool wasInAttackPosition;
    private float arrivedTimer;

    private Animator anim;
    private EnemyHealth enemyHealth;

    private Vector2 desiredOffset;
    private float offsetTimer;
    private float cooldownTimer;
    private float currentRetreatDistance;

    private bool canMove;
    private bool facingRight = true;

    private void Awake()
    {
        startingScale = transform.localScale;
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    private void Start()
    {
        PlayAnimationState(idleAnimationState);

        Invoke(nameof(EnableMovement), movementStartDelay);
        GenerateNewOffset();
    }

    private void Update()
    {
        if (player == null) return;

        cooldownTimer += Time.deltaTime;

        FacePlayer();

        if (isAttacking)
            return;

        if (enemyHealth != null && enemyHealth.IsStunned)
            return;

        HandleMovementAndAttack();
        RefreshOffsetIfNeeded();
    }

    private void HandleMovementAndAttack()
    {
        if (IsTooCloseToPlayer())
        {
            RetreatFromPlayer();
            return;
        }

        if (IsInAttackPosition())
        {
            if (CanAttackAfterArrivalDelay())
            {
                Attack();
            }

            return;
        }

        if (canMove)
        {
            MoveToAttackPosition();
        }
    }

    private bool CanAttackAfterArrivalDelay()
    {
        bool inAttackPosition = IsInAttackPosition();

        if (!inAttackPosition)
        {
            wasInAttackPosition = false;
            arrivedTimer = 0f;
            return false;
        }

        if (!wasInAttackPosition)
        {
            wasInAttackPosition = true;
            arrivedTimer = 0f;
            return false;
        }

        arrivedTimer += Time.deltaTime;

        return arrivedTimer >= attackDelayAfterArriving;
    }

    private void RefreshOffsetIfNeeded()
    {
        if (IsInAttackPosition()) return;

        offsetTimer += Time.deltaTime;

        if (offsetTimer < positionRefreshTime) return;

        offsetTimer = 0f;
        GenerateNewOffset();
    }

    private void GenerateNewOffset()
    {
        float side = transform.position.x < player.position.x ? -1f : 1f;

        desiredOffset = new Vector2(
            desiredXDistance * side,
            Random.Range(-maxYVariation, maxYVariation)
        );

        currentRetreatDistance = Random.Range(
            retreatDistanceMin,
            retreatDistanceMax
        );
    }

    private Vector3 GetDesiredAttackPosition()
    {
        return new Vector3(
            player.position.x + desiredOffset.x,
            player.position.y + desiredOffset.y,
            transform.position.z
        );
    }

    private bool IsInAttackPosition()
    {
        Vector3 targetPosition = GetDesiredAttackPosition();

        float xDistance = Mathf.Abs(transform.position.x - targetPosition.x);
        float yDistance = Mathf.Abs(transform.position.y - targetPosition.y);

        return xDistance <= xTolerance && yDistance <= yTolerance;
    }

    private bool IsTooCloseToPlayer()
    {
        float xDistance = Mathf.Abs(transform.position.x - player.position.x);
        return xDistance < minimumPersonalSpace;
    }

    private void MoveToAttackPosition()
    {
        MoveTowards(GetDesiredAttackPosition());
    }

    private void RetreatFromPlayer()
    {
        float side = transform.position.x < player.position.x ? -1f : 1f;

        Vector3 retreatTarget = transform.position;
        retreatTarget.x += side * currentRetreatDistance;

        MoveTowards(retreatTarget);
    }

    private void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }

    private void FacePlayer()
    {
        facingRight = player.position.x > transform.position.x;

        float x = Mathf.Abs(startingScale.x);

        if (!facingRight)
            x *= -1f;

        transform.localScale = new Vector3(
            x,
            startingScale.y,
            startingScale.z
        );
    }

    private void Attack()
    {
        if (isAttacking) return;
        if (cooldownTimer < attackCooldown) return;

        arrivedTimer = 0f;
        wasInAttackPosition = false;

        cooldownTimer = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGruntAttack();
        }

        StartCoroutine(AttackRoutine());


    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Debug.Log($"{name} attacks.");

        PlayAnimationState(attackWindupAnimationState);

        yield return new WaitForSeconds(attackWindupTime);

        PlayAnimationState(attackHitAnimationState);

        HitPlayer();

        yield return new WaitForSeconds(attackHitFrameTime);

        PlayAnimationState(idleAnimationState);

        isAttacking = false;
    }

    private void PlayAnimationState(string stateName)
    {
        if (anim == null) return;
        if (string.IsNullOrEmpty(stateName)) return;

        anim.Play(stateName, 0, 0f);
    }

    private void HitPlayer()
    {
        Vector2 hitboxCenter = GetAttackHitboxCenter();

        Collider2D hit = Physics2D.OverlapBox(
            hitboxCenter,
            attackHitboxSize,
            0f,
            playerLayer
        );

        if (hit == null)
        {
            Debug.Log($"{name} missed.");
            return;
        }

        Health playerHealth = hit.GetComponent<Health>();

        if (playerHealth == null)
        {
            Debug.LogWarning("Player was hit, but no Health component was found.");
            return;
        }

        playerHealth.TakeDamage(attackDamage);

        Debug.Log($"{name} hit player for {attackDamage} damage.");
    }

    private Vector2 GetAttackHitboxCenter()
    {
        float direction = facingRight ? 1f : -1f;

        Vector2 offset = attackHitboxOffset;
        offset.x *= direction;

        return (Vector2)transform.position + offset;
    }

    private void EnableMovement()
    {
        canMove = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetDesiredAttackPosition(), 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackHitboxCenter(), attackHitboxSize);
    }
}