using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController1 : MonoBehaviour
{
    public GameObject LoseWindow;

    private void Start()
    {
        // Убедимся, что окно проигрыша скрыто при старте
        if (LoseWindow != null)
            LoseWindow.SetActive(false);
    }

    public void PlayGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void OpenMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void Lose()
    {
        if (LoseWindow != null)
            LoseWindow.SetActive(true);
    }

    public void Rewarde()
    {
        if (LoseWindow != null)
            LoseWindow.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }
}