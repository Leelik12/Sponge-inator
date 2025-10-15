using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel = null;
    [SerializeField] private GameObject settingsPanel = null;
    [SerializeField] private GameObject soundPanel = null;
    [SerializeField] private GameObject DifficultyPanel = null;
    [SerializeField] private GameObject SensetivetyPanel = null;

    [Header("Buttons")]
    [SerializeField] private Button playButton = null;
    [SerializeField] private Button settingsButton = null;
    [SerializeField] private Button soundButton = null;

    [SerializeField] private Button SensetivetyButton = null;

    [Header("Back Buttons")]
    [SerializeField] private Button backButtonSettings = null;
    [SerializeField] private Button backButtonsound = null;
    [SerializeField] private Button backButtondifficulty = null;
    [SerializeField] private Button backButtonSensetivety = null;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Awake()
    {
        // Главное меню всегда активно изначально
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        soundPanel.SetActive(false);
        SensetivetyPanel.SetActive(false);
        DifficultyPanel.SetActive(false);

        // Назначаем переходы на кнопки
        settingsButton.onClick.AddListener(() => OpenPanel(settingsPanel));
        soundButton.onClick.AddListener(() => OpenPanel(soundPanel));
        SensetivetyButton.onClick.AddListener(() => OpenPanel(SensetivetyPanel));
        playButton.onClick.AddListener(() => OpenPanel(DifficultyPanel));

        // Назначаем обработчики на backButton
        backButtonSettings.onClick.AddListener(GoBack);
        backButtonsound.onClick.AddListener(GoBack);
        backButtonSensetivety.onClick.AddListener(GoBack);
    }

    private void OpenPanel(GameObject targetPanel)
    {
        // Добавляем текущую активную панель в историю (кроме главного меню)
        if (GetActivePanel() != mainMenuPanel)
        {
            panelHistory.Push(GetActivePanel());
        }

        // Показываем целевую панель и скрываем остальные
        ShowOnlyPanel(targetPanel);

    }
    
    public void GoToMain()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        soundPanel.SetActive(false);
        SensetivetyPanel.SetActive(false);
        DifficultyPanel.SetActive(false);
    }
    private void GoBack()
    {
        if (panelHistory.Count > 0)
        {
            // Возвращаемся к предыдущей панели
            GameObject previousPanel = panelHistory.Pop();
            ShowOnlyPanel(previousPanel);
        }
        else
        {
            // Если история пуста, возвращаемся в главное меню
            ShowOnlyPanel(mainMenuPanel);
            Debug.Log("Returned to main menu");
        }
    }

    private void ShowOnlyPanel(GameObject panelToShow)
    {
        // Скрываем все панели
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        soundPanel.SetActive(false);
        SensetivetyPanel.SetActive(false);

        // Показываем только нужную панель
        panelToShow.SetActive(true);
    }

    private GameObject GetActivePanel()
    {
        if (settingsPanel.activeSelf) return settingsPanel;
        if (soundPanel.activeSelf) return soundPanel;
        if (SensetivetyPanel.activeSelf) return SensetivetyPanel;
        return mainMenuPanel;
    }
}