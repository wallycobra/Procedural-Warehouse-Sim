using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SoundEntry
    {
        public SoundType soundType;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<SoundEntry> sounds = new();

    private Dictionary<SoundType, SoundEntry> soundLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        soundLookup = new Dictionary<SoundType, SoundEntry>();

        foreach (SoundEntry sound in sounds)
        {
            soundLookup[sound.soundType] = sound;
        }
    }

    public void PlaySfx(SoundType soundType)
    {

        if (!soundLookup.TryGetValue(soundType, out SoundEntry sound))
        {
            Debug.LogWarning($"Missing sound: {soundType}");
            return;
        }
        
        if (sound == null || sound.clip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogWarning($"{sound.clip.name} is not loaded yet.");
            return;
        }

        sfxSource.PlayOneShot(sound.clip, sound.volume);
    }
    public void PlayButtonClick()
    {
        PlaySfx(SoundType.ButtonClick);
    }
}