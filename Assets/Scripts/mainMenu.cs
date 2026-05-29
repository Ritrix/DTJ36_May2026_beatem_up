using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene("TestScene");
    }

    //public void tutorial()
    //{
    //    SceneManager.LoadScene("Tutorial");
    //}
}
