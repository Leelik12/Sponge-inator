using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;


    // Настройки громкости
    private const float MIN_VOLUME_DB = -50f;
    private const float MAX_VOLUME_DB = 20f;

    // Имена для поиска слайдеров
    private const string MUSIC_SLIDER_NAME = "MusicSlider";
    private const string SFX_SLIDER_NAME = "SFXSlider";


    public static SettingsManager Instance { get; private set; }

    // Имена параметров Audio Mixer
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    // Флаги для отслеживания инициализации
    private bool isInitialized = false;
    private bool slidersFound = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSlidersInScene();
        SubscribeToSliders();
        SyncSlidersWithAudioMixer();
        ApplyLoadedSettings();
    }

    private void FindSlidersInScene()
    {
        slidersFound = false;

        // Сбрасываем ссылки на слайдеры перед поиском
        musicSlider = null;
        sfxSlider = null;

        // Поиск по имени
        GameObject musicObj = GameObject.Find(MUSIC_SLIDER_NAME);
        GameObject sfxObj = GameObject.Find(SFX_SLIDER_NAME);

        if (musicObj != null) musicSlider = musicObj.GetComponent<Slider>();
        if (sfxObj != null) sfxSlider = sfxObj.GetComponent<Slider>();

        //// Если не нашли по имени, ищем по тегам
        //if (musicSlider == null)
        //{
        //    GameObject musicByTag = GameObject.FindWithTag("MusicSlider");
        //    if (musicByTag != null) musicSlider = musicByTag.GetComponent<Slider>();
        //}

        //if (sfxSlider == null)
        //{
        //    GameObject sfxByTag = GameObject.FindWithTag("SFXSlider");
        //    if (sfxByTag != null) sfxSlider = sfxByTag.GetComponent<Slider>();
        //}

        // Резервный поиск через Canvas
        if (musicSlider == null || sfxSlider == null)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Canvas canvas in allCanvases)
            {
                Slider[] canvasSliders = canvas.GetComponentsInChildren<Slider>(true);

                foreach (Slider slider in canvasSliders)
                {
                    if (slider.name == "MusicSlider" && musicSlider == null)
                        musicSlider = slider;
                    else if (slider.name == "SFXSlider" && sfxSlider == null)
                        sfxSlider = slider;
                }
            }
        }

        // Устанавливаем флаг если нашли хотя бы один слайдер
        slidersFound = (musicSlider != null || sfxSlider != null);

        // Настраиваем диапазоны найденных слайдеров
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 100f;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 100f;
        }
    }

    private void InitializeSettings()
    {
        if (isInitialized) return;

        LoadSettings();
        SubscribeToSliders();

        // Синхронизация с AudioMixer при первом запуске
        if (!PlayerPrefs.HasKey("MusicVolume") || !PlayerPrefs.HasKey("SFXVolume"))
        {
            SyncSlidersWithAudioMixer();
        }

        isInitialized = true;
    }

    private void SubscribeToSliders()
    {
        // Отписываемся от старых событий
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            musicSlider.onValueChanged.AddListener(_ => SaveSettings());
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxSlider.onValueChanged.AddListener(_ => SaveSettings());
        }
    }

    private void SyncSlidersWithAudioMixer()
    {
        // Синхронизация музыки
        if (musicSlider != null && audioMixer != null)
        {
            if (audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out float currentMusicDB))
            {
                float musicPercent = DBToPercent(currentMusicDB);
                musicSlider.SetValueWithoutNotify(musicPercent);
                PlayerPrefs.SetFloat("MusicVolume", musicPercent);
            }
        }

        // Синхронизация SFX
        if (sfxSlider != null && audioMixer != null)
        {
            if (audioMixer.GetFloat(SFX_VOLUME_PARAM, out float currentSfxDB))
            {
                float sfxPercent = DBToPercent(currentSfxDB);
                sfxSlider.SetValueWithoutNotify(sfxPercent);
                PlayerPrefs.SetFloat("SFXVolume", sfxPercent);
            }
        }

        PlayerPrefs.Save();
    }

    private float DBToPercent(float dbValue)
    {
        if (dbValue <= MIN_VOLUME_DB) return 0f;
        if (dbValue >= MAX_VOLUME_DB) return 100f;

        // Обратное преобразование dB в проценты
        float normalized = (dbValue - MIN_VOLUME_DB) / (MAX_VOLUME_DB - MIN_VOLUME_DB);
        float percent = Mathf.Pow(normalized, 2f) * 100f;
        return Mathf.Clamp(percent, 0f, 100f);
    }

    private void LoadSettings()
    {
        // Загружаем музыку
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 75f);
        if (musicSlider != null)
            musicSlider.value = musicVolume;

        // Загружаем SFX
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 75f);
        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;
    }

    private void ApplyLoadedSettings()
    {
        if (musicSlider != null)
            SetMusicVolume(musicSlider.value, true);

        if (sfxSlider != null)
            SetSFXVolume(sfxSlider.value, true);
    }

    public void SaveSettings()
    {
        if (musicSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);

        if (sfxSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        PlayerPrefs.Save();
    }

    // Audio Methods
    public void SetMusicVolume(float volume) => SetMusicVolume(volume, false);
    public void SetSFXVolume(float volume) => SetSFXVolume(volume, false);

    private void SetMusicVolume(float volume, bool silent)
    {
        if (audioMixer == null) return;

        float volumeDB = CalculateVolumeDB(volume);

        if (silent && musicSlider != null)
            musicSlider.SetValueWithoutNotify(volume);

        try
        {
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, volumeDB);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set music volume: {e.Message}");
        }
    }

    private void SetSFXVolume(float volume, bool silent)
    {
        if (audioMixer == null) return;

        float volumeDB = CalculateVolumeDB(volume);

        if (silent && sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(volume);

        try
        {
            audioMixer.SetFloat(SFX_VOLUME_PARAM, volumeDB);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set SFX volume: {e.Message}");
        }
    }

    // Основной метод преобразования громкости
    private float CalculateVolumeDB(float volumePercent)
    {
        if (volumePercent <= 0f) return MIN_VOLUME_DB;
        if (volumePercent >= 100f) return MAX_VOLUME_DB;

        float normalizedVolume = volumePercent / 100f;
        float curvedVolume = Mathf.Pow(normalizedVolume, 0.5f);
        return Mathf.Lerp(MIN_VOLUME_DB, MAX_VOLUME_DB, curvedVolume);
    }


    // Public method для принудительного обновления слайдеров
    public void RefreshSliders()
    {
        FindSlidersInScene();
        SubscribeToSliders();
        ApplyLoadedSettings();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveSettings();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}