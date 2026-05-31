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

        GUI.Box(new Rect(5, 5, 330, 220), "Debug");

        GUI.Label(new Rect(15, 30, 300, 20),
            $"Coins: {GameManager.Instance.Coins}");

        GUI.Label(new Rect(15, 50, 300, 20),
            $"Wave: {GameManager.Instance.CurrentWave}");

        GUI.Label(new Rect(15, 70, 300, 20),
            $"Bonus Health: {GameManager.Instance.BonusMaxHealth}");

        GUI.Label(new Rect(15, 90, 300, 20),
            $"Bonus Damage: {GameManager.Instance.BonusDamage}");

        GUI.Label(new Rect(15, 110, 300, 20),
            $"Bonus Armour: {GameManager.Instance.BonusArmour}");

        GUI.Label(new Rect(15, 130, 300, 20),
            $"Bonus Speed: {GameManager.Instance.BonusMoveSpeed}");

        GUI.Label(new Rect(15, 150, 300, 20),
            $"Held Item: {GameManager.Instance.HeldItem}");

        GUI.Label(new Rect(15, 170, 300, 20),
            $"Coin Magnetism: {GameManager.Instance.HasCoinMagnetism}");

        if (playerHealth != null)
        {
            GUI.Label(new Rect(15, 190, 300, 20),
                $"Player HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        }
    }
}