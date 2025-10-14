using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorMenuButtons : MonoBehaviour
{
    [Header("References")]
    public GameObject menuCanvas; // Ссылка на канвас меню

    // Кнопка 1: Выйти в главное меню (переход на другую сцену)
    public void GoToMainMenu()
    {
        // Сначала снимаем паузу
        Time.timeScale = 1f;

        // Разблокируем курсор для меню
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Загружаем сцену главного меню
        // ЗАМЕНИ "MainMenu" на имя твоей сцены с главным меню!
        SceneManager.LoadScene("MainMenu");
    }

    // Кнопка 2: Закрыть меню и остаться в текущей сцене
    public void CloseMenuAndStay()
    {
        // Просто скрываем канвас меню
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        // Снимаем паузу - возобновляем игру
        Time.timeScale = 1f;

        // Блокируем и скрываем курсор для геймплея
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Меню закрыто, игра продолжается");
    }
}