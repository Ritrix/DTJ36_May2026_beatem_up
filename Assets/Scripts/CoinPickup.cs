using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int value = 1;

    [Header("Arc")]
    [SerializeField] private float minFlightTime = 0.5f;
    [SerializeField] private float maxFlightTime = 1f;
    [SerializeField] private float minArcHeight = 0.7f;
    [SerializeField] private float maxArcHeight = 1.8f;

    [Header("Landing")]
    [SerializeField] private float minLandingDistance = 0.6f;
    [SerializeField] private float maxLandingDistance = 2.2f;
    [SerializeField] private float maxVerticalLandingOffset = 0.35f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color bronzeColor = new Color(0.75f, 0.35f, 0.15f);
    [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color goldColor = new Color(1f, 0.75f, 0.1f);
    [SerializeField] private Color redColor = new Color(1f, 0.1f, 0.1f);

    [SerializeField] private float magnetDelay = 2f;
    [SerializeField] private float magnetSpeed = 6f;

    private float landedTimer;
    private Transform playerTarget;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float flightTime;
    private float arcHeight;
    private float timer;

    private bool isFlying;
    private bool canPickup;

    public void SetValue(int newValue)
    {
        value = newValue;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        switch (value)
        {
            case 1:
                spriteRenderer.color = bronzeColor;
                break;

            case 10:
                spriteRenderer.color = silverColor;
                break;

            case 25:
                spriteRenderer.color = goldColor;
                break;

            case 100:
                spriteRenderer.color = redColor;
                break;
        }
    }

    public void Launch(Vector2 spawnPosition, int horizontalDirection)
    {
        startPosition = spawnPosition;

        horizontalDirection = horizontalDirection >= 0 ? 1 : -1;

        float randomX = Random.Range(minLandingDistance, maxLandingDistance);
        randomX *= horizontalDirection;

        float randomY = Random.Range(-maxVerticalLandingOffset, maxVerticalLandingOffset);

        targetPosition = startPosition + new Vector3(randomX, randomY, 0f);

        flightTime = Random.Range(minFlightTime, maxFlightTime);
        arcHeight = Random.Range(minArcHeight, maxArcHeight);

        timer = 0f;
        isFlying = true;
        canPickup = false;

        transform.position = startPosition;
    }

    private void Update()
    {
        if (!isFlying) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / flightTime);

        Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, t);

        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = flatPosition + Vector3.up * arc;

        if (t >= 1f)
        {
            Land();
        }

        if (!isFlying && canPickup)
        {
            HandleMagnetism();
        }
    }

    private void Land()
    {
        isFlying = false;
        canPickup = true;
        transform.position = targetPosition;

        landedTimer = 0f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTarget = player.transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Coin collided with {other.name}, can pickup: {canPickup}");
        if (!canPickup) return;

        PlayerWallet wallet = other.GetComponent<PlayerWallet>();
        Debug.Log($"wallet found: {wallet != null}");
        if (wallet == null) return;

        wallet.AddCoins(value);
        Destroy(gameObject);
    }

    private void HandleMagnetism()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.HasCoinMagnetism &&
            !GameManager.Instance.HasPerk(PerkType.Wildcard))
            return;

        landedTimer += Time.deltaTime;

        if (landedTimer < magnetDelay) return;
        if (playerTarget == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerTarget.position,
            magnetSpeed * Time.deltaTime
        );
    }
}