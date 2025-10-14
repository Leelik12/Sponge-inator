using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject MainCanvas;
    public GameObject SettingsCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Settings()
    {
        MainCanvas.SetActive(false);
        SettingsCanvas.SetActive(true);
    }
    public void Back2Main()
    {
        MainCanvas.SetActive(true);
        SettingsCanvas.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
