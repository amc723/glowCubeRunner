using UnityEngine;
using System.Collections;

abstract public class AbstPlayerInputManager : MonoBehaviour
{
	abstract public bool IsJumping
	{
		get;
	}

	abstract public float HorizontalInput
	{
		get;
	}

	abstract public bool PlayBackgroundMusic
	{
		get;
	}
}

// handles all input related things for the player cube
public class PlayerInputManager : AbstPlayerInputManager
{
	protected bool isJumping = true;			// player starts in the air so initialize true
	protected bool playBackgroundMusic = true;	// player can hit "m" key to turn background music on and off, for the current play session. TODO: make this option persistent
	public const float maxJumpTime = .1f;		// jumping should be variable in length dependent on length of key press. maxJumpTime specifies max time key press is considered
	protected float currJumpTime;				// keeps track of how long a jump key has been pressed
	protected float horizontalInput;			// value is retained while jumping so player can't change horizontal velocity in air

	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.M))
			playBackgroundMusic = !playBackgroundMusic;

		if (!isJumping && Input.GetKeyDown (KeyCode.Space))
		{
			rigidbody.constraints = rigidbody.constraints & ~RigidbodyConstraints.FreezePositionY;	// unfreeze y position
			isJumping = true;
			currJumpTime = 0f;
		}

		// player continues jumping as long as key is pressed and maxJumpTime not reached
		else if (currJumpTime < maxJumpTime && Input.GetKey (KeyCode.Space))
		{
			currJumpTime += Time.deltaTime;

			// slowly decrease horizontal input to prevent speeding up while in air
			horizontalInput *= .9f;
		}

		// if player let go of space key then end jumping
		else if (isJumping)
		{
			currJumpTime = maxJumpTime + .01f;

			// slowly decrease horizontal input to prevent speeding up while in air
			horizontalInput *= .9f;
		}

		// only allow horizontal movement change if not jumping
		else
		{
			horizontalInput = Input.GetAxis ("Horizontal");
		}
	}

	void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.tag)
		{
		case "Ground":
			isJumping = false;
			break;
			
		case "DropPlane":
			isJumping = true;
			break;
		}
	}

	override public bool IsJumping
	{
		// for physics manager, forces from jumping only occur while jump key is pressed and maxJumpTime not exceeded
		get { return isJumping && currJumpTime < maxJumpTime; }
	}

	override public bool PlayBackgroundMusic
	{
		get { return playBackgroundMusic; }
	}

	override public float HorizontalInput
	{
		get { return horizontalInput; }
	}
}