using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void returnToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenuAfterDeath();
    }
}
