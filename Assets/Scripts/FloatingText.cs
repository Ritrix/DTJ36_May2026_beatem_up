using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float arcHeight = 0.3f;

    private float totalLifetime;
    private Vector3 startPosition;

    private Vector3 moveDirection;
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 999;
        }
    }



    public void Initialize(string message, Color color, Vector3 direction)
    {
        text.text = message;
        text.color = color;

        moveDirection = direction.normalized;

        totalLifetime = lifeTime;
        startPosition = transform.position;
    }

    private void Update()
    {
        float t = 1f - (lifeTime / totalLifetime);

        Vector3 horizontalMovement =
            moveDirection * moveSpeed * totalLifetime * t;

        float arc =
            Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position =
            startPosition +
            horizontalMovement +
            Vector3.up * arc;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
            Destroy(gameObject);
    }
}