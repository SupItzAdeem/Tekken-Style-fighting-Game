using UnityEngine;

public enum SoundType
{
    BGM,
    JUMP,
    PUNCH,
    KICK,
    SUPERKICK,
    TAKEDAMAGE,
    WALK,
    HURRICANE,
    GAMEOVER,
}


public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    private static SoundManager instance;

    private AudioSource audioSource;
    private AudioSource walkingSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            walkingSource = gameObject.AddComponent<AudioSource>();
            walkingSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        int index = (int)sound;

        if (instance.soundList == null || index >= instance.soundList.Length || instance.soundList[index] == null)
        {
            Debug.LogWarning($"❗ SoundManager: Missing or invalid AudioClip for '{sound}' at index {index}.");
            return;
        }

        instance.audioSource.PlayOneShot(instance.soundList[index], volume);
    }

    public static void PlaySoundLoop(SoundType sound)
    {
        int index = (int)sound;

        if (instance.soundList == null || index >= instance.soundList.Length || instance.soundList[index] == null)
        {
            Debug.LogWarning($"❗ SoundManager: Missing loopable clip for '{sound}' at index {index}.");
            return;
        }

        instance.audioSource.clip = instance.soundList[index];
        instance.audioSource.loop = true;
        instance.audioSource.volume = 1f;
        instance.audioSource.Play();
    }

    public static void PlayLoopedSound(SoundType sound)
    {
        int index = (int)sound;

        if (instance.soundList == null || index >= instance.soundList.Length || instance.soundList[index] == null)
        {
            Debug.LogWarning($"❗ SoundManager: Missing looped clip for '{sound}' at index {index}.");
            return;
        }

        instance.audioSource.Stop();
        instance.audioSource.clip = instance.soundList[index];
        instance.audioSource.loop = true;
        instance.audioSource.volume = 1f;
        instance.audioSource.Play();
    }

    public static void PlayWalkingSound()
    {
        if (!instance.walkingSource.isPlaying && instance.soundList.Length > (int)SoundType.WALK)
        {
            instance.walkingSource.clip = instance.soundList[(int)SoundType.WALK];
            instance.walkingSource.Play();
        }
    }

    public static void StopWalkingSound()
    {
        if (instance.walkingSource.isPlaying)
        {
            instance.walkingSource.Stop();
        }
    }

    public static void StopMusic()
    {
        instance.audioSource.Stop();
    }
}
