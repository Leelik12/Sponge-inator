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
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SetNextRandomTimer();
    }

    void Update()
    {
        if (randomClips.Length > 0 && !isPlaying)
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
        if (isPlaying) return; // Не воспроизводить, если уже играет другая фраза

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        isPlaying = true;
        audioSource.PlayOneShot(clip);

        // Запустить корутину для отслеживания окончания звука
        StartCoroutine(WaitForClipToEnd(clip.length));
    }

    private System.Collections.IEnumerator WaitForClipToEnd(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        isPlaying = false;
    }

    // ------------------- Методы для действий -------------------

    public void SayZhirDamage() => PlayRandomClip(ZhirdamageClips);
    public void SayAttack() => PlayRandomClip(attackClips);
    public void SaypickupWater() => PlayRandomClip(pickupWaterClips);
    public void SaypickupSoap() => PlayRandomClip(pickupSoapClips);
    public void SaypickupGas() => PlayRandomClip(pickupGasClips);
    public void SaypickupZhir() => PlayRandomClip(pickupZhirClips);
    public void SayenemyDied() => PlayRandomClip(enemyDiedClips);

    public void SayOther() => PlayRandomClip(otherClips);

    /// <summary>
    /// Проигрывает конкретный аудиоклип
    /// </summary>
    public void SayClip(AudioClip clip)
    {
        if (clip != null && !isPlaying)
        {
            isPlaying = true;
            audioSource.PlayOneShot(clip);
            StartCoroutine(WaitForClipToEnd(clip.length));
        }
    }

    /// <summary>
    /// Проигрывает случайную фразу из списка случайных клипов
    /// </summary>
    public void SayRandom()
    {
        PlayRandomClip(randomClips);
    }
}