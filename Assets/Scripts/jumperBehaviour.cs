using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JumperBehaviour : MonoBehaviour
{
    private enum JumperState
    {
        Hidden,
        FadingIn,
        Dropping,
        Windup,
        Dashing,
        Recovering,
        Stunned
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Vector2 detectionSize = new Vector2(2f, 8f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(0f, -4f);

    [Header("Drop Attack")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float dropSpeed = 10f;
    [SerializeField] private float landingYOffset = 0.2f;

    [Header("Dash Attack")]
    [SerializeField] private float windupTime = 0.75f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashPastPlayerDistance = 3f;
    [SerializeField] private float recoverTime = 0.8f;

    [Header("Damage")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float hitCooldownPerDash = 0.25f;

    [Header("Launched When Hit")]
    [SerializeField] private float launchedDistance = 3f;
    [SerializeField] private float launchedDuration = 0.25f;
    [SerializeField] private float getUpDelay = 0.75f;

    [Header("Telegraph Hops")]
    [SerializeField] private int windupHopCount = 3;
    [SerializeField] private float windupHopDistance = 0.45f;
    [SerializeField] private float windupHopHeight = 0.35f;
    [SerializeField] private float windupHopDuration = 0.25f;

    [Header("Dive Arc")]
    [SerializeField] private float diveDuration = 0.55f;
    [SerializeField] private float diveHeight = 1.2f;

    [Header("Survival Auto Drop")]
    [SerializeField] private bool autoDropInSurvival = true;
    [SerializeField] private float autoDropDelay = 10f;

    private float hiddenTimer;

    private JumperState state = JumperState.Hidden;

    private bool hasHitPlayerThisAttack;
    private float hitTimer;
    private Vector2 dashTarget;

    public bool CanBeKnockedDown =>
        state == JumperState.Dropping ||
        state == JumperState.Windup ||
        state == JumperState.Dashing;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        SetAlpha(0.05f);
    }

    private void Update()
    {
        if (player == null) return;

        hitTimer -= Time.deltaTime;

        switch (state)
        {
            case JumperState.Hidden:
                HandleHiddenState();
                break;

            case JumperState.Dropping:
                HandleDrop();
                break;

            case JumperState.Windup:
                FacePlayer();
                break;

        }
    }

    private void HandleHiddenState()
    {
        hiddenTimer += Time.deltaTime;

        if (PlayerBelow())
        {
            hiddenTimer = 0f;
            StartCoroutine(FadeInThenDrop());
            return;
        }

        if (autoDropInSurvival && hiddenTimer >= autoDropDelay)
        {
            hiddenTimer = 0f;
            ForceDropNearPlayer();
        }
    }

    private void ForceDropNearPlayer()
    {
        if (player == null) return;

        Vector3 dropPosition = transform.position;

        dropPosition.x = player.position.x + Random.Range(-1.5f, 1.5f);
        dropPosition.y = player.position.y + 5f;
        dropPosition.z = transform.position.z;

        transform.position = dropPosition;

        Debug.Log($"{name} auto-dropping after hidden timeout.");

        StartCoroutine(FadeInThenDrop());
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    private IEnumerator FadeInThenDrop()
    {
        state = JumperState.FadingIn;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            SetAlpha(timer / fadeDuration);
            yield return null;
        }

        SetAlpha(1f);

        hasHitPlayerThisAttack = false;
        state = JumperState.Dropping;
    }

    private void HandleDrop()
    {
        float targetY = player.position.y + landingYOffset;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, dropSpeed * Time.deltaTime);
        transform.position = pos;

        TryHitPlayer();

        if (Mathf.Abs(transform.position.y - targetY) <= 0.05f)
        {
            StartCoroutine(WindupThenDash());
        }
    }

    private IEnumerator WindupThenDash()
    {
        state = JumperState.Windup;

        for (int i = 0; i < windupHopCount; i++)
        {
            FacePlayer();

            float direction = i % 2 == 0 ? -1f : 1f;

            Vector2 start = transform.position;
            Vector2 end = start + new Vector2(direction * windupHopDistance, 0f);

            yield return StartCoroutine(ArcMove(
                start,
                end,
                windupHopHeight,
                windupHopDuration,
                false
            ));
        }

        hasHitPlayerThisAttack = false;

        float attackDirection = player.position.x > transform.position.x ? 1f : -1f;

        Vector2 dashStart = transform.position;

        dashTarget = new Vector2(
            player.position.x + attackDirection * dashPastPlayerDistance,
            player.position.y
        );

        yield return StartCoroutine(ArcMove(
            dashStart,
            dashTarget,
            diveHeight,
            diveDuration,
            true
        ));

        StartCoroutine(RecoverThenWindup());
    }

    private IEnumerator ArcMove(
        Vector2 start,
        Vector2 end,
        float height,
        float duration,
        bool canDamage
    )
    {
        state = canDamage ? JumperState.Dashing : JumperState.Windup;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            Vector2 position = Vector2.Lerp(start, end, t);

            float arc = Mathf.Sin(t * Mathf.PI) * height;
            position.y += arc;

            transform.position = position;

            if (canDamage)
            {
                TryHitPlayer();
            }

            yield return null;
        }

        transform.position = end;
    }

    private void HandleDash()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            dashTarget,
            dashSpeed * Time.deltaTime
        );

        TryHitPlayer();

        if (Vector2.Distance(transform.position, dashTarget) <= 0.05f)
        {
            StartCoroutine(RecoverThenWindup());
        }
    }

    private IEnumerator RecoverThenWindup()
    {
        state = JumperState.Recovering;

        yield return new WaitForSeconds(recoverTime);

        StartCoroutine(WindupThenDash());
    }

    private void TryHitPlayer()
    {
        if (hasHitPlayerThisAttack) return;
        if (hitTimer > 0f) return;

        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            0.5f,
            playerLayer
        );

        if (hit == null) return;

        Health health = hit.GetComponentInParent<Health>();

        if (health == null) return;

        health.TakeDamage(contactDamage);

        hasHitPlayerThisAttack = true;
        hitTimer = hitCooldownPerDash;

        Debug.Log($"{name} body-slammed player for {contactDamage} damage.");
    }

    public void LaunchAwayFromPlayer()
    {
        Debug.Log($"{name} LaunchAwayFromPlayer called. State: {state}");

        if (state == JumperState.Stunned) return;

        StopAllCoroutines();
        StartCoroutine(LaunchRoutine());
    }

    private IEnumerator LaunchRoutine()
    {
        state = JumperState.Stunned;

        hasHitPlayerThisAttack = true;

        Vector2 start = transform.position;

        float direction = transform.position.x < player.position.x ? -1f : 1f;

        Vector2 end = start + new Vector2(direction * launchedDistance, 0f);

        float knockbackHeight = 0.8f;

        float timer = 0f;

        while (timer < launchedDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / launchedDuration);

            Vector2 position = Vector2.Lerp(start, end, t);

            float arc = Mathf.Sin(t * Mathf.PI) * knockbackHeight;
            position.y += arc;

            transform.position = position;

            yield return null;
        }

        transform.position = end;

        Debug.Log($"{name} knocked down.");

        yield return new WaitForSeconds(getUpDelay);

        hasHitPlayerThisAttack = false;

        StartCoroutine(WindupThenDash());
    }

    private bool PlayerBelow()
    {
        Vector2 center = (Vector2)transform.position + detectionOffset;

        Collider2D hit = Physics2D.OverlapBox(
            center,
            detectionSize,
            0f,
            playerLayer
        );

        return hit != null;
    }

    private void FacePlayer()
    {
        bool facingRight = player.position.x > transform.position.x;

        float xScale = facingRight ? 2f : -2f;
        transform.localScale = new Vector3(xScale, 2f, 2f);
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 center = (Vector2)transform.position + detectionOffset;
        Gizmos.DrawWireCube(center, detectionSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}