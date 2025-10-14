using UnityEngine;

public class TriggerMenu : MonoBehaviour
{
    public GameObject menuCanvas; // Ссылка на канвас меню

    private void Start()
    {
        // Скрываем меню при старте
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        // Гарантируем, что триггер включен при старте
        this.gameObject.SetActive(true);
    }

    private void Update()
    {
        // Если триггер почему-то отключился - включаем его
        if (!this.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Триггер был отключен! Включаем обратно.");
            this.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер вошел игрок
        if (other.CompareTag("Player"))
        {
            ShowMenu();
        }
    }

    void ShowMenu()
    {
        // Активируем канвас
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
        }

        // Ставим игру на паузу
        Time.timeScale = 0f;

        // Разблокируем и показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Скрываем меню
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        // Возобновляем игру
        Time.timeScale = 1f;

        // Блокируем и скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}