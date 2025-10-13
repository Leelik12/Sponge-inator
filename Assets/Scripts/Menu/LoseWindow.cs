using UnityEngine;

public class LoseWindow : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private GameObject LoseWindows;
    [SerializeField] private MenuController1 Gamemanager;
    [SerializeField] private MenuToggle menuToggle;

    public void Update()
    {

        if (LoseWindows != null)
        {
            bool isMenuOpening = LoseWindows.activeSelf;
            LoseWindows.SetActive(isMenuOpening);

            // Управляем аудио в зависимости от состояния меню
            if (isMenuOpening)
            {
                menuToggle.ForcePauseAudio();
            }
            else
            {
                menuToggle.ForceResumeAudio();
            }
            // Обновляем состояние курсора
            UpdateCursorState(isMenuOpening);

        }
        
    }

    private void UpdateCursorState(bool menuOpen)
    {
        if (menuOpen)
        {
            Gamemanager.Pause();
            UnlockCursor(); // Разблокируем курсор для меню
        }
        else
        {
            Gamemanager.Resume();
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
}
