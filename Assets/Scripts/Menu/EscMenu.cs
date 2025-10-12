using UnityEngine;
using UnityEngine.Audio;

public class MenuToggle : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private Canvas menuCanvas; // Ссылка на Canvas меню
    [SerializeField] private bool startHidden = true; // Скрыть меню при старте
    [SerializeField] private MenuController1 MenuController;
    //[SerializeField] private FirstPersonLook firstPersonLook;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer; // Ссылка на Audio Mixer
    [SerializeField] private string masterVolumeParameter = "MasterVolume"; // Параметр громкости в Audio Mixer

    private float savedMasterVolume = 0f; // Сохраненное значение громкости
    private bool isAudioPaused = false; // Флаг паузы аудио

    private void Start()
    {
        // Изначально скрываем или показываем меню
        if (menuCanvas != null)
        {
            menuCanvas.enabled = !startHidden;
            UpdateCursorState(!startHidden);
        }

        // Изначально скрываем курсор и блокируем его
        if (startHidden)
        {
            LockCursor();
        }

        // Сохраняем начальную громкость
        SaveCurrentVolume();
    }

    private void Update()
    {
        // Проверяем нажатие Esc, но только если окно проигрыша не активно
        if (Input.GetKeyDown(KeyCode.Escape) && !IsLoseWindowActive())
        {
            ToggleMenu();
        }
    }

    // Метод для переключения видимости меню
    public void ToggleMenu()
    {
        if (menuCanvas != null)
        {
            bool isMenuOpening = !menuCanvas.enabled;
            menuCanvas.enabled = isMenuOpening;

            // Управляем аудио в зависимости от состояния меню
            if (isMenuOpening)
            {
                PauseAudio();
            }
            else
            {
                ResumeAudio();
            }

            // Обновляем состояние курсора
            UpdateCursorState(isMenuOpening);
        }
    }

    // Пауза аудио
    private void PauseAudio()
    {
        if (audioMixer != null && !isAudioPaused)
        {
            // Сохраняем текущую громкость
            SaveCurrentVolume();

            // Устанавливаем очень низкую громкость (почти беззвучно)
            audioMixer.SetFloat(masterVolumeParameter, -80f);
            isAudioPaused = true;
        }
    }

    // Возобновление аудио
    private void ResumeAudio()
    {
        if (audioMixer != null && isAudioPaused)
        {
            // Восстанавливаем сохраненную громкость
            audioMixer.SetFloat(masterVolumeParameter, savedMasterVolume);
            isAudioPaused = false;
        }
    }

    // Сохранение текущей громкости
    private void SaveCurrentVolume()
    {
        if (audioMixer != null)
        {
            if (audioMixer.GetFloat(masterVolumeParameter, out float currentVolume))
            {
                savedMasterVolume = currentVolume;
            }
            else
            {
                // Значение по умолчанию, если не удалось получить текущее
                savedMasterVolume = 0f;
            }
        }
    }

    // Обновление состояния курсора
    private void UpdateCursorState(bool menuOpen)
    {
        if (menuOpen)
        {
            MenuController.Pause();
            //firstPersonLook.enabled = false;
            UnlockCursor(); // Разблокируем курсор для меню
        }
        else
        {
            MenuController.Resume();
            //firstPersonLook.enabled = true;
            LockCursor(); // Блокируем курсор для игры
        }
    }

    // Разблокировать курсор (для меню)
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Курсор свободен
        Cursor.visible = true; // Курсор виден
    }

    // Заблокировать курсор (для игры)
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Курсор заблокирован в центре
        Cursor.visible = false; // Курсор не виден
    }

    // Метод для принудительной паузы аудио (можно вызвать из других скриптов)
    public void ForcePauseAudio()
    {
        PauseAudio();
    }

    // Метод для принудительного возобновления аудио
    public void ForceResumeAudio()
    {
        ResumeAudio();
    }

    // Метод для проверки состояния аудио
    public bool IsAudioPaused()
    {
        return isAudioPaused;
    }

    // Метод для получения сохраненной громкости
    public float GetSavedVolume()
    {
        return savedMasterVolume;
    }

    // Метод для принудительной блокировки курсора (например, из UI кнопки)
    public void ForceLockCursor()
    {
        LockCursor();
    }

    private void OnDestroy()
    {
        // При уничтожении объекта возобновляем аудио на всякий случай
        if (isAudioPaused)
        {
            ResumeAudio();
        }
    }

    // Метод для проверки активности окна проигрыша
    private bool IsLoseWindowActive()
    {
        // Ищем GameObject с тегом "LoseWindow"
        GameObject loseWindow = GameObject.FindGameObjectWithTag("LoseWindow");

        if (loseWindow != null)
        {
            // Проверяем активен ли GameObject в иерархии
            return loseWindow.activeInHierarchy;
        }

        return false;
    }
}