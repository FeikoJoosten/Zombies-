using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	private List<AudioSource> sfxAudio = new List<AudioSource>();
	private List<AudioSource> musicAudio = new List<AudioSource>();

	private float savedSFXVolume = 100;
	private float savedMusicVolume = 100;

	public float SavedSFXVolume
	{
		get { return savedSFXVolume; }
		set { savedSFXVolume = value; }
	}
	public float SavedMusicVolume
	{
		get { return savedMusicVolume; }
		set { savedSFXVolume = value; }
	}

	private void Awake()
	{
		if (PlayerPrefs.HasKey("SFXVolume") == true && PlayerPrefs.HasKey("MusicVolume") == true)
		{
			savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
			savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume");

			UpdateAudioVolumes(savedSFXVolume, savedMusicVolume);
		}
		else
		{
			UpdateAudioVolumes(100, 100);
		}

	}

	public void AddSFXAudioSource(AudioSource source)
	{
		source.volume = savedSFXVolume;
		sfxAudio.Add(source);
	}

	public void AddMusicAudioSource(AudioSource source)
	{
		source.volume = savedMusicVolume;
		musicAudio.Add(source);
	}

	public void PlaySFXSound(AudioSource sourceToPlayFrom, AudioClip clipToPlay)
	{
		if (sourceToPlayFrom.clip != clipToPlay)
		{
			sourceToPlayFrom.clip = clipToPlay;
		}

		if (sourceToPlayFrom.volume != savedSFXVolume)
		{
			sourceToPlayFrom.volume = savedSFXVolume;
		}

		if (savedSFXVolume == 0)
		{
			return;
		}

		sourceToPlayFrom.Play();
	}

	public void PlayMusicSound(AudioSource sourceToPlayFrom, AudioClip clipToPlay)
	{
		if (savedMusicVolume == 0)
		{
			return;
		}
		if (sourceToPlayFrom.clip != clipToPlay)
		{
			sourceToPlayFrom.clip = clipToPlay;
		}

		if (sourceToPlayFrom.volume != savedMusicVolume)
		{
			sourceToPlayFrom.volume = savedMusicVolume;
		}

		sourceToPlayFrom.Play();
	}

	public void UpdateAudioVolumes(float sFXVolume, float musicVolume)
	{
		foreach (AudioSource sfxaudio in sfxAudio)
		{
			if (sfxaudio == null)
			{
				continue;
			}

			sfxaudio.volume = sFXVolume;
		}

		foreach (AudioSource musicaudio in musicAudio)
		{
			if (musicaudio == null)
			{
				continue;
			}

			musicaudio.volume = musicVolume;
		}

		savedSFXVolume = sFXVolume;
		savedMusicVolume = musicVolume;

		PlayerPrefs.SetFloat("SFXVolume", savedSFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", savedMusicVolume);
		PlayerPrefs.Save();
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetFloat("SFXVolume", savedSFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", savedMusicVolume);
		PlayerPrefs.Save();
	}
}
