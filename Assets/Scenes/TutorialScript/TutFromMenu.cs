using UnityEngine;
using UnityEngine.SceneManagement;

public class TutFromMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
