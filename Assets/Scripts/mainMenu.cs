using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void startSurvivalModeIntro()
    {
        SceneManager.LoadScene("SurvivalModeIntro");
    }

    public void startSurvivalMode()
    {
        SceneManager.LoadScene("SurvivalMode");
    }

    //public void tutorial()
    //{
    //    SceneManager.LoadScene("Tutorial");
    //}
}
