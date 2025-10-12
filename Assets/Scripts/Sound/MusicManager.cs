using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MusicManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Playback Settings")]
    [SerializeField] private bool shuffle = true;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool waitForUserInteraction = true;

    private List<int> playlist = new List<int>();
    private int currentTrackIndex = -1;
    private bool isFading = false;
    private bool audioInitialized = false;
    private bool isWaitingForClick = true;

    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            // Останавливаем автовоспроизведение при старте
            if (waitForUserInteraction)
            {
                audioSource.Stop();
                isWaitingForClick = true;

                // Добавляем коллайдер для захвата кликов, если его нет
                if (GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
                {
                    gameObject.AddComponent<BoxCollider2D>().size = Vector2.one * 0.1f;
                }
            }
            else
            {
                InitializeAudio();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Обработчик кликов для активации аудио
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isWaitingForClick)
        {
            InitializeAudio();
            isWaitingForClick = false;
        }
    }

    // Альтернативный метод активации через любую кнопку
    private void Update()
    {
        // Активация по любому клику/тапу
        if (isWaitingForClick && Input.GetMouseButtonDown(0))
        {
            InitializeAudio();
            isWaitingForClick = false;
        }

        if (audioInitialized && !isFading && !audioSource.isPlaying && currentTrackIndex >= 0)
        {
            PlayNextTrack();
        }
    }

    // Инициализация аудио системы
    public void InitializeAudio()
    {
        if (audioInitialized) return;

        CreatePlaylist();
        PlayRandomTrack();
        audioInitialized = true;

    }

    // Создание плейлиста
    private void CreatePlaylist()
    {
        playlist.Clear();

        for (int i = 0; i < musicTracks.Length; i++)
        {
            playlist.Add(i);
        }

        if (shuffle)
        {
            ShufflePlaylist();
        }
    }

    // Остальные методы остаются без изменений...
    private void ShufflePlaylist()
    {
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = playlist[i];
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }

    private void PlayRandomTrack()
    {
        if (musicTracks.Length == 0) return;

        currentTrackIndex = shuffle ? Random.Range(0, playlist.Count) : 0;
        StartCoroutine(FadeToNextTrack());
    }

    public void PlayNextTrack()
    {
        if (!audioInitialized || musicTracks.Length == 0) return;

        int nextIndex = currentTrackIndex + 1;

        if (nextIndex >= playlist.Count)
        {
            if (loopPlaylist)
            {
                nextIndex = 0;
                if (shuffle)
                {
                    ShufflePlaylist();
                }
            }
            else
            {
                currentTrackIndex = -1;
                return;
            }
        }

        currentTrackIndex = nextIndex;
        StartCoroutine(FadeToNextTrack());
    }

    private IEnumerator FadeToNextTrack()
    {
        isFading = true;

        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        int trackIndex = playlist[currentTrackIndex];
        audioSource.clip = musicTracks[trackIndex];

        float targetVolume = audioSource.volume;
        audioSource.volume = 0f;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isFading = false;

    }

    // Модифицированные методы с проверкой инициализации
    public void SkipTrack()
    {
        if (audioInitialized && !isFading)
        {
            PlayNextTrack();
        }
    }

    public void StartRandomPlayback()
    {
        if (!audioInitialized) return;

        if (shuffle)
        {
            ShufflePlaylist();
            currentTrackIndex = Random.Range(0, playlist.Count);
        }
        else
        {
            currentTrackIndex = 0;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        StartCoroutine(FadeToNextTrack());
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            currentTrackIndex = -1;
        }
    }

    public void PlayMusic()
    {
        if (!audioInitialized) return;

        if (!audioSource.isPlaying && currentTrackIndex >= 0)
        {
            audioSource.Play();
        }
        else if (currentTrackIndex < 0)
        {
            PlayNextTrack();
        }
    }

    // Принудительная активация аудио из других скриптов
    public void ForceInitializeAudio()
    {
        if (!audioInitialized)
        {
            InitializeAudio();
            isWaitingForClick = false;
        }
    }
}