using UnityEngine;
using System.Collections;

public class PlayerCameraFollower : MonoBehaviour 
{
	public Transform follow;
	public float followDistance;
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, follow.position.z - followDistance);
	}
}
