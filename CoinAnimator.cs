using UnityEngine;
using System.Collections;

public class CoinAnimator : MonoBehaviour 
{
	public float rotateSpeed;

	void FixedUpdate()
	{
		transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
	}
}
