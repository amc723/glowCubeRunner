using UnityEngine;
using System.Collections;

abstract public class AbstPlayerPhysicsManager : MonoBehaviour
{
	// PlayerPhysics actually has no dependencies, but this is here for consistency anyway
}

// handles all physics related things for the player cube
public class PlayerPhysicsManager :  AbstPlayerPhysicsManager
{
	protected AbstPlayerInputManager inputManager;
	protected AbstPlayerStatsManager statsManager;
	public float horizontalSpeed;
	public float jumpForce = 40f;					// determines how high and how quickly the player can jump
	public float speedIncreaseRate;	// how fast to increase forwardSpeed based on IPlayerInput's distance travelled
	public float baseForwardSpeed = 10f;		// actual forward speed = baseForwardSpeed + distanceTravelled * speedIncreaseRate

	public void Start()
	{
		inputManager = gameObject.GetComponent<AbstPlayerInputManager> ();
		statsManager = gameObject.GetComponent<AbstPlayerStatsManager> ();
	}

	// physics updates here
	public void FixedUpdate()
	{
		// set upward force if jumping
		if (inputManager.IsJumping)
		{
			rigidbody.AddForce (gameObject.transform.up * jumpForce);
		}

		// set horizontal force
		rigidbody.AddForce(gameObject.transform.right * inputManager.HorizontalInput * horizontalSpeed);

		// set forward velocity
		float forwardSpeed = baseForwardSpeed + statsManager.DistanceTravelled * speedIncreaseRate;
		rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, forwardSpeed);
	}

	public void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.tag)
		{
		case "Ground":
			rigidbody.constraints = rigidbody.constraints | RigidbodyConstraints.FreezePositionY;	// needed because sliding the cube across tile gaps occasionally causes the cube to jump
			break;

		// dropPlanes tell the physics manager that a drop is present somewhere, so allow the cube to fall
		case "DropPlane":
			rigidbody.constraints = rigidbody.constraints & ~RigidbodyConstraints.FreezePositionY;	// unfreeze y position
			break;
		}
	}
}