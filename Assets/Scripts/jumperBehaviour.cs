using UnityEngine;


public class jumperBehaviour : MonoBehaviour
{
    [Header("Follow Player + Movement")]
    public GameObject playerPosition;
    public GameObject enemyObject;
    [SerializeField] private bool facingRight = true;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Vector2 detectionSize = new Vector2(3f, 2f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(3f, -1f);
    //[SerializeField] private float detectWidth = 2f;
    //[SerializeField] private float detectHeight = 1f;
    [SerializeField] private float dropSpeed = 10f;
    private bool hasLanded;
    private float dropTargetY; // locked Y to move toward

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    private float cooldownTimer;
    private bool isDropping;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hasLanded = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Color tmp = enemyObject.GetComponent<SpriteRenderer>().color;
        //tmp.a = 0f;
        //enemyObject.GetComponent<SpriteRenderer>().color = tmp;
    }

    // Update is called once per frame
    void Update()
    {
        //facePlayer();

        cooldownTimer += Time.deltaTime;

        Debug.Log($"is dropping: {isDropping}");

        if (!isDropping && PlayerBelow())
        {
            isDropping = true;
            dropTargetY = playerPosition.transform.position.y - 1f;
            // optional: stop horizontal movement here
            Debug.Log("Player detected below → dropping!");
        }

        if (isDropping && !hasLanded)
        {
            Drop();
            
        }

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

    //private void facePlayer() //faces sprite towards player onx axis
    //{
    //    // face towards player
    //    if (playerPosition.transform.position.x < enemyObject.transform.position.x)
    //    {
    //        // Player is to the left
    //        facingRight = false;
    //        enemyObject.transform.localScale = new Vector3(2f, 2f, 2f); // Normal scale
    //    }
    //    else
    //    {
    //        // Player is to the right
    //        facingRight = true;
    //        enemyObject.transform.localScale = new Vector3(-2f, 2f, 2f); // Flip x scale
    //    }
    //}

    // ---------------- PLAYER DETECTION ----------------

    private bool PlayerInSight()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        Vector2 detectPosition = (Vector2)transform.position;

        detectPosition += new Vector2(
            direction.x * detectionOffset.x,
            detectionOffset.y
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            detectPosition,
            detectionSize,
            0f,
            Vector2.zero,
            0f,
            playerLayer
        );



        return hit.collider != null;
    }


    // ---------------- GIZMOS ----------------

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Calculate final detection box position
        Vector2 detectPosition = (Vector2)transform.position + detectionOffset;

        // Draw rectangle for visualization
        Gizmos.DrawWireCube(detectPosition, detectionSize);
    }
}
