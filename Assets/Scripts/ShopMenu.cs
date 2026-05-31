using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void startNextRound()
    {
        GameManager.Instance.StartNextWave();
    }
}
