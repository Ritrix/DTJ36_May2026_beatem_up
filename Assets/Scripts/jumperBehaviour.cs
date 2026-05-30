using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class jumperBehaviour : MonoBehaviour
{
    [Header("Follow Player + Movement")]
    public GameObject playerPosition;
    public GameObject enemyObject;
    [SerializeField] private bool facingRight = true;
    private bool isBouncing;
    private float bounceTimer;
    private Vector2 bounceDirection;
    [SerializeField] private float bounceDistance = 0.5f;
    [SerializeField] private float bounceDuration = 1f;
    private bool canMove;

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Vector2 detectionSize = new Vector2(3f, 2f);
    [SerializeField] private Vector3 detectionRange = new Vector3(2f, 0f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(3f, -1f);
    [SerializeField] private Vector2 landedDetectionSize = new Vector2(3f, 2f);
    //[SerializeField] private Vector3 landedDetectionRange = new Vector3(2f, 0f);
    //[SerializeField] private Vector2 landedDetectionOffset = new Vector2(3f, -1f);
    //[SerializeField] private float detectWidth = 2f;
    //[SerializeField] private float detectHeight = 1f;
    [SerializeField] private float dropSpeed = 10f;
    private bool hasLanded;
    private float dropTargetY; // locked Y to move toward
    private bool playerWasInSight = false;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 2f;
    private float cooldownTimer;
    private bool isDropping;
    private Rigidbody2D rb;
    private bool isAttacking;
    private bool recharging;
    [SerializeField] public BoxCollider2D attackHitbox;


    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hasLanded = false;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Start invisible
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        StartCoroutine(FadeInAfterDelay());

    }

    private IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            spriteRenderer.color = color;

            yield return null;

            canMove = true;
        }

        // Ensure fully visible
        Color finalColor = spriteRenderer.color;
        finalColor.a = 1f;
        spriteRenderer.color = finalColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove)
            return;

        //facePlayer();

        cooldownTimer += Time.deltaTime;

        //Debug.Log($"is dropping: {isDropping}");

        if (!isDropping && PlayerBelow())
        {
            isDropping = true;
            dropTargetY = playerPosition.transform.position.y - 1f;
            // optional: stop horizontal movement here
            Debug.Log("Player detected below → dropping!");
        }

        if (isBouncing)
        {
            enemyObject.transform.position +=
                (Vector3)(bounceDirection * (bounceDistance / bounceDuration) * Time.deltaTime);

            bounceTimer -= Time.deltaTime;

            if (bounceTimer <= 0f)
            {
                isBouncing = false;
            }
        }

        if (isDropping && !hasLanded)
        {
            Drop();

        }
        facePlayer();

        if (hasLanded && PlayerInSight() && !isAttacking && !recharging)
        {
            StartCoroutine(AttackRoutine());
        }
        Debug.Log($"Has landed: {hasLanded}, player in sight: {PlayerInSight()}, Is attacking: {isAttacking}, recharging: {recharging}");
    }


    
    
    // JUMP ATTACK SEQUENCE //
    private IEnumerator AttackRoutine()
    {


        isAttacking = true;
        recharging = true;



        // Telegraph attack
        yield return StartCoroutine(WindupHops());



        // Lock player's position
        Vector2 attackTarget = playerPosition.transform.position;




        // Attack
        yield return StartCoroutine(
            ArcJump(
                transform.position,
                attackTarget,
                3f,
                0.8f
            )
        );

        if (attackHitbox.enabled == true)
        {
            Debug.Log("Jump attack Hit");
        }



        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        recharging = false;


    }

    //actual attack
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (!isAttacking)
    //    {
    //        return;
    //    }
        
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Jump Attack Hit");
    //    }
    //}

    private IEnumerator WindupHops()
    {
        float hopDistance = 0.5f;
        float hopHeight = 0.4f;
        float hopDuration = 0.5f;

        for (int i = 0; i < 3; i++)
        {
            float direction = (i % 2 == 0) ? -1f : 1f;

            Vector2 start = transform.position;
            Vector2 end = start + Vector2.right * direction * hopDistance;

            yield return StartCoroutine(
                ArcJump(start, end, hopHeight, hopDuration)
            );
        }
    }

    private IEnumerator ArcJump(
    Vector2 startPos,
    Vector2 endPos,
    float jumpHeight,
    float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            // Move from start to end
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);

            // Add arc
            float arc = jumpHeight * 4f * t * (1f - t);

            pos.y += arc;

            transform.position = pos;

            yield return null;
        }

        transform.position = endPos;
    }

    private void Drop()
    {


        Vector3 pos = enemyObject.transform.position;


        pos.y = Mathf.MoveTowards(
                pos.y,
                dropTargetY,
                5f * Time.deltaTime
                );

        if (pos.y <= dropTargetY)
        {
            hasLanded = true;

        }

        enemyObject.transform.position = pos;

        // Randomly bounce left or right
        float dir = Random.value < 0.5f ? -1f : 1f;

        bounceDirection = Vector2.right * dir;
        isBouncing = true;
        bounceTimer = bounceDuration;

    }

    private bool PlayerBelow()
    {
        Vector2 detectPosition = (Vector2)transform.position + detectionOffset;

        Collider2D hit = Physics2D.OverlapBox(
            detectPosition,
            detectionSize,
            0f,
            playerLayer
        );

        return hit != null;
    }

    private void facePlayer() //faces sprite towards player on x axis
    {
        // face towards player
        if (playerPosition.transform.position.x<enemyObject.transform.position.x)
        {
            // Player is to the left
            facingRight = false;
            enemyObject.transform.localScale = new Vector3(-2f, 2f, 2f); // flip x scale
}
        else
        {
            // Player is to the right
            facingRight = true;
            enemyObject.transform.localScale = new Vector3(2f, 2f, 2f); // normal scale
        }
    }

    private void jumpAttack()
    {

    }

    // ---------------- LANDED PLAYER DETECTION ----------------

    private bool PlayerInSight()
    {

        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            landedDetectionSize,
            0f,
            Vector2.zero,
            0f,
            playerLayer
        );


        bool playerInSight = hit.collider != null;

        // Player just entered the detection box
        if (playerInSight && !playerWasInSight)
        {
            Debug.Log("Player detected, jumping!");
        }

        playerWasInSight = playerInSight;

        return hit.collider != null;

    }


    // ---------------- GIZMOS ----------------

    private void OnDrawGizmos()
    {



        if (hasLanded)
        {
            Gizmos.color = Color.red;

            Vector2 detectPosition = (Vector2)transform.position;

            Vector2 direction = facingRight ? Vector2.right : Vector2.left;

            detectPosition += new Vector2(
                direction.x * detectionOffset.x,
                detectionOffset.y
            );

            Gizmos.DrawWireCube(detectPosition, detectionRange);

            // Large rectangle centered on enemy
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, landedDetectionSize);
        }
        else
        {
            Gizmos.color = Color.red;

            // Calculate final detection box position
            Vector2 detectPosition = (Vector2)transform.position + detectionOffset;

            // Draw rectangle for visualization
            Gizmos.DrawWireCube(detectPosition, detectionSize);

        }
        
    }
}
