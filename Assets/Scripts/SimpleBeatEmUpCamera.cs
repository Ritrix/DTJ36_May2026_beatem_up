using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleBeatEmUpCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    private Camera cam;

    [Header("Follow")]
    [SerializeField] private bool useDeadzone = true;
    [SerializeField] private float followSmooth = 10f;
    [SerializeField] private Vector2 followOffset;

    [Header("Deadzone")]
    [SerializeField, Range(0f, 0.49f)] private float deadzoneX = 0.35f;
    [SerializeField, Range(0f, 0.49f)] private float deadzoneY = 0.25f;

    [Header("Zoom")]
    [SerializeField] private float targetZoom = 7f;
    [SerializeField] private float zoomSmooth = 8f;
    [SerializeField] private float targetZoomReset = 7f;

    [Header("Screen Shake")]
    [SerializeField] private float maxShake = 0.35f;
    [SerializeField] private float shakeDecay = 8f;

    private Vector3 basePosition;
    private float shakeAmount;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        basePosition = transform.position;

        if (!cam.orthographic)
        {
            Debug.LogWarning("SimpleBeatEmUpCamera expects an orthographic camera.");
        }

        //targetZoom = cam.orthographicSize;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float dt = Time.deltaTime;

        basePosition = useDeadzone
            ? GetDeadzoneFollowPosition(basePosition, dt)
            : GetDirectFollowPosition(basePosition, dt);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            1f - Mathf.Exp(-zoomSmooth * dt)
        );

        Vector3 shakeOffset = Vector3.zero;

        if (shakeAmount > 0.001f)
        {
            shakeOffset = (Vector3)(Random.insideUnitCircle * shakeAmount);
            shakeAmount = Mathf.MoveTowards(shakeAmount, 0f, shakeDecay * dt);
        }

        transform.position = basePosition + shakeOffset;
    }

    private Vector3 GetDirectFollowPosition(Vector3 currentPosition, float dt)
    {
        Vector3 target = new Vector3(
            player.position.x + followOffset.x,
            player.position.y + followOffset.y,
            currentPosition.z
        );

        float followT = 1f - Mathf.Exp(-followSmooth * dt);
        return Vector3.Lerp(currentPosition, target, followT);
    }

    private Vector3 GetDeadzoneFollowPosition(Vector3 currentPosition, float dt)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float deadzoneWidth = halfWidth * (1f - deadzoneX);
        float deadzoneHeight = halfHeight * (1f - deadzoneY);

        Vector3 target = currentPosition;

        Vector3 playerPosition = new Vector3(
            player.position.x + followOffset.x,
            player.position.y + followOffset.y,
            currentPosition.z
        );

        float left = currentPosition.x - deadzoneWidth;
        float right = currentPosition.x + deadzoneWidth;
        float bottom = currentPosition.y - deadzoneHeight;
        float top = currentPosition.y + deadzoneHeight;

        if (playerPosition.x < left)
            target.x -= left - playerPosition.x;

        if (playerPosition.x > right)
            target.x += playerPosition.x - right;

        if (playerPosition.y < bottom)
            target.y -= bottom - playerPosition.y;

        if (playerPosition.y > top)
            target.y += playerPosition.y - top;

        float followT = 1f - Mathf.Exp(-followSmooth * dt);
        return Vector3.Lerp(currentPosition, target, followT);
    }

    public void SetZoom(float newZoom)
    {
        targetZoom = newZoom;
    }

    public void ResetZoom()
    {
        targetZoom = targetZoomReset;
    }

    public void Shake(float amount)
    {
        shakeAmount = Mathf.Min(maxShake, shakeAmount + amount);
    }
}