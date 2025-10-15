using UnityEngine;

public class StartScreen : MonoBehaviour
{
    [Header("Стартовый экран")]
    public GameObject startScreenCanvas; // Ссылка на канвас стартового экрана

    [Header("Другие канвасы (станут активны после старта)")]
    public GameObject[] otherCanvases; // Массив других канвасов

    void Start()
    {

        // Делаем неактивными другие канвасы при запуске
        SetOtherCanvasesActive(false);

        // Показываем канвас при запуске сцены
        if (startScreenCanvas != null)
        {
            startScreenCanvas.SetActive(true);

            // Ставим игру на паузу (опционально)
            Time.timeScale = 0f;

            // Разблокируем курсор для кнопок
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogError("[StartScreen] Start Canvas не назначен в инспекторе!");
        }
    }

    /// <summary>
    /// Метод для кнопки "Начать игру" - закрывает стартовый экран
    /// </summary>
    public void CloseStartScreen()
    {
        // Скрываем стартовый канвас
        if (startScreenCanvas != null)
        {
            startScreenCanvas.SetActive(false);
        }

        // Делаем активными другие канвасы
        SetOtherCanvasesActive(true);

        // Возобновляем игру (если ставили на паузу)
        Time.timeScale = 1f;

        // Блокируем курсор для геймплея (опционально)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Стартовый экран закрыт, другие канвасы активированы!");
    }

    /// <summary>
    /// Включает или выключает другие канвасы
    /// </summary>
    /// <param name="isActive">true - включить, false - выключить</param>
    private void SetOtherCanvasesActive(bool isActive)
    {
        if (otherCanvases != null && otherCanvases.Length > 0)
        {
            foreach (GameObject canvas in otherCanvases)
            {
                if (canvas != null)
                {
                    canvas.SetActive(isActive);
                }
            }
            Debug.Log($"Другие канвасы установлены в состояние: {(isActive ? "активны" : "неактивны")}");
        }
    }

    // Для отладки - можно вызвать из инспектора
    [ContextMenu("Показать стартовый экран")]
    public void ShowStartScreen()
    {
        SetOtherCanvasesActive(false);
        if (startScreenCanvas != null)
        {
            startScreenCanvas.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}