using UnityEngine;
using System.Collections;

abstract public class AbstPlayerStatsManager : MonoBehaviour
{
	abstract public float DistanceTravelled
	{
		get;
	}

	abstract public int CoinCount
	{
		get;
	}

	abstract public bool GameOver
	{
		get;
	}

	abstract public float PrevZPosition
	{
		get;
		set;
	}
}

// handles player's coin count and distance travelled
// audio and respawn code has been lumped in here as well, since those weren't large enough components to warrant their own classes, and UI communicates with stats manager anyway
public class PlayerStatsManager : AbstPlayerStatsManager
{
	AudioManager audioManager;
	protected int coinCount = 0;
	public float prevZPosition;					// used to determine distanceTravelled, public for use with the map generator, in case the whole map is ever pushed back
	protected float distanceTravelled = 0f;
	protected bool gameOver;

	void Start () 
	{
		prevZPosition = transform.position.z;
		audioManager = GameObject.FindGameObjectWithTag ("MainCamera").transform.FindChild ("AudioPlayer").GetComponent<AudioManager> ();
	}

	void Update () 
	{
		// update distanceTravelled
		float currZPosition = transform.position.z;
		distanceTravelled += transform.position.z - prevZPosition;
		prevZPosition = currZPosition;
	}

	void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.tag)
		{
		case "Respawn":
			gameOver = true;
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			break;
			
		case "Coin":
			coinCount += 1;
			DestroyObject(other.gameObject);
			audioManager.PlayCoinPickupSound();
			break;
		}
	}
	
	override public float DistanceTravelled
	{
		get { return distanceTravelled; }
	}

	override public int CoinCount
	{
		get { return coinCount; }
	}

	override public bool GameOver
	{
		get { return gameOver; }
	}

	override public float PrevZPosition
	{
		get { return prevZPosition; }
		set { prevZPosition = value; }
	}
}
