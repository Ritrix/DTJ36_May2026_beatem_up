using UnityEngine;

public class DebugRunInfo : MonoBehaviour
{
    private Health playerHealth;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null)
            return;

        GUI.Box(new Rect(5, 5, 260, 115), "Debug");

        GUI.Label(new Rect(15, 30, 240, 20),
            $"Coins: {GameManager.Instance.Coins}");

        GUI.Label(new Rect(15, 50, 240, 20),
            $"Wave: {GameManager.Instance.CurrentWave}");

        GUI.Label(new Rect(15, 70, 240, 20),
            $"Bonus Max Health: {GameManager.Instance.BonusMaxHealth}");

        if (playerHealth != null)
        {
            GUI.Label(new Rect(15, 90, 240, 20),
                $"Player Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        }
    }
}
