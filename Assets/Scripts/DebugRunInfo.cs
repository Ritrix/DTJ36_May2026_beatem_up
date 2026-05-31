using UnityEngine;

public class DebugRunInfo : MonoBehaviour
{
    private void OnGUI()
    {
        if (GameManager.Instance == null)
            return;

        GUI.Label(
            new Rect(10, 10, 300, 25),
            $"Coins: {GameManager.Instance.Coins}"
        );

        GUI.Label(
            new Rect(10, 30, 300, 25),
            $"Wave: {GameManager.Instance.CurrentWave}"
        );
    }
}