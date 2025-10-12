using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HeroVoice : MonoBehaviour
{
    [Header("Audio Source Settings")]
    private AudioSource audioSource;

    [Header("Voice Clips By Action")]
    public AudioClip[] ZhirdamageClips;
    public AudioClip[] attackClips;
    public AudioClip[] pickupWaterClips;
    public AudioClip[] pickupSoapClips;
    public AudioClip[] pickupGasClips;
    public AudioClip[] pickupZhirClips;
    public AudioClip[] enemyDiedClips;
    public AudioClip[] otherClips;

    [Header("Random Idle Phrases")]
    [Tooltip("Список случайных фраз, которые герой произносит сам по себе")]
    public AudioClip[] randomClips;

    [Tooltip("Минимальный интервал между случайными фразами")]
    public float minInterval = 5f;

    [Tooltip("Максимальный интервал между случайными фразами")]
    public float maxInterval = 15f;

    private float randomTimer;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SetNextRandomTimer();
    }

    void Update()
    {
        if (randomClips.Length > 0)
        {
            randomTimer -= Time.deltaTime;
            if (randomTimer <= 0f)
            {
                SayRandom();
                SetNextRandomTimer();
            }
        }
    }

    private void SetNextRandomTimer()
    {
        randomTimer = Random.Range(minInterval, maxInterval);
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    // ------------------- Методы для действий -------------------

    public void SayJump() => PlayRandomClip(jumpClips);
    public void SayAttack() => PlayRandomClip(attackClips);
    public void SayPickup() => PlayRandomClip(pickupClips);
    public void SayOther() => PlayRandomClip(otherClips);

    /// <summary>
    /// Проигрывает конкретный аудиоклип
    /// </summary>
    public void SayClip(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Проигрывает случайную фразу из списка случайных клипов
    /// </summary>
    public void SayRandom()
    {
        PlayRandomClip(randomClips);
    }
}
