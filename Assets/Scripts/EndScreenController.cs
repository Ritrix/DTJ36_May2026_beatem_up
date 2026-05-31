using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{
    public void ReturnToMenu()
    {
        Debug.Log("Return to menu button clicked.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenuAfterDeath();
        }
        else
        {
            Debug.LogWarning("No GameManager found. Loading MainMenuScene directly.");
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}