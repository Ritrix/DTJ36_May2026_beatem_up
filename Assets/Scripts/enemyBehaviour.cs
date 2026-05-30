using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;


public class enemyBehaviour : MonoBehaviour
{
    [Header("Follow Player + Movement")]
    public GameObject playerPosition;
    public GameObject enemyObject;
    [SerializeField] private bool facingRight = true;
    [SerializeField] private float minYOffset = -0.1f;
    [SerializeField] private float maxYOffset = +0.1f;
    private float targetYOffset;

    [Header("Detection")]
    [SerializeField] private Vector3 detectionRange = new Vector3 (2f, 0f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(3f, 0f);
    [SerializeField] private LayerMask playerLayer; 


    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    private float cooldownTimer;


    private Animator anim;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public float enemySpeed = 1f;

    private bool canMove;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(EnableMovement), 5f);

        //get random y offset 
        targetYOffset = Random.Range(minYOffset, maxYOffset);
    }

    private void EnableMovement()
    {
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        facePlayer();
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            // Player detected → stop and attack
            Attack();
        }
        else if (canMove) 
        {
            
            // Move toward player + offset
            Vector3 targetPos = new Vector3(
                playerPosition.transform.position.x,
                playerPosition.transform.position.y + targetYOffset,
                enemyObject.transform.position.z
            );

            enemyObject.transform.position = Vector3.MoveTowards(
                enemyObject.transform.position,
                targetPos,
                enemySpeed * Time.deltaTime
            );
        }
    }



    private void facePlayer() //faces sprite towards player on x axis
    {
        // face towards player
        if (playerPosition.transform.position.x < enemyObject.transform.position.x)
        {
            // Player is to the left
            facingRight = false;
            enemyObject.transform.localScale = new Vector3(2f, 2f, 2f); // Normal scale
        }
        else
        {
            // Player is to the right
            facingRight= true;
            enemyObject.transform.localScale = new Vector3(-2f, 2f, 2f); // Flip x scale
        }
    }

    // ---------------- ATTACK ----------------

    private void Attack()
    {

        if (cooldownTimer >= attackCooldown)
        {
            Debug.Log("Attack triggered");

            cooldownTimer = 0f;
            anim.SetTrigger("meleeAttack");
        }
    }

    // ---------------- PLAYER DETECTION ----------------

    private bool PlayerInSight()
    {

        Vector2 detectPosition = (Vector2)transform.position;


        Collider2D hit = Physics2D.OverlapBox(
            detectPosition,
            detectionRange,
            0f,
            playerLayer
        );


        return hit != null;
    }


    // ---------------- GIZMOS ----------------

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector2 detectPosition = (Vector2)transform.position;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        detectPosition += new Vector2(
            direction.x * detectionOffset.x,
            detectionOffset.y
        );

        Gizmos.DrawWireCube(detectPosition, detectionRange);

    }
}
