using UnityEngine;
using System.Collections;

// TODO: give this a better structure
public class AudioManager : MonoBehaviour 
{
	protected AbstPlayerInputManager playerInput;		// tells audio manager to turn background music on/off
	protected AudioClip currentSong;
	protected AudioSource backgroundMusicAudio;
	protected AudioSource coinPickupAudio;
	protected static readonly string [] songs = new string[]
	{
		"1-11 Space Cadet",
		"03 Meeker Warm Energy",
		"03 Tell Your Mother",
		"04 All That Matters",
		"Bring Back the Funk",
		"11 Ten Tigers",
		"16 Civil (Battle)"
	};
	protected bool [] playedSongs = new bool[7];		// for a shuffle playlist
	protected int songsPlayed = 0;
	protected bool quiet = true;

	protected void SetSong(string songName)
	{
		if (quiet)
			return;
		backgroundMusicAudio.clip = Resources.Load<AudioClip>("audio/" + songName);
		backgroundMusicAudio.Play();
	}

	public void PlayCoinPickupSound()
	{
		if (quiet)
			return;
		coinPickupAudio.Play ();
	}

	void Start ()
	{
		playerInput = GameObject.FindGameObjectWithTag ("Player").GetComponent<AbstPlayerInputManager> ();
		coinPickupAudio = gameObject.AddComponent<AudioSource>();
		coinPickupAudio.clip = Resources.Load<AudioClip>("audio/CoinPickupSound");
		coinPickupAudio.volume = .6f;

		for (int i = 0; i < playedSongs.Length; i++)
			playedSongs [i] = false;

		backgroundMusicAudio = gameObject.AddComponent<AudioSource>();
		int songNumber = Random.Range(0, songs.Length);
		SetSong(songs [songNumber]);
		playedSongs [songNumber] = true;

		// 
	}

	void Update () 
	{
		if (!playerInput.PlayBackgroundMusic && backgroundMusicAudio.isPlaying)
		{
			backgroundMusicAudio.Stop ();
		}

		else if (!backgroundMusicAudio.isPlaying)
		{
			songsPlayed += 1;
			int songNumber = Random.Range(0, songs.Length);

			// all songs have been played, reset playedSongs
			if (songsPlayed == 7)
			{
				for (int i = 0; i < playedSongs.Length; i++)
					playedSongs [i] = false;
			}

			// find the next song that hasn't been played yet
			while (playedSongs[songNumber] == true)
				songNumber = (songNumber + 1) % (songs.Length - 1);

			SetSong(songs[songNumber]);
		}
	}
}
