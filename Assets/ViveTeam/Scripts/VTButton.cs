using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VTButton : MonoBehaviour
{
	private bool isPressed;
	//private Collider _collider;
	public bool IsPresssed()
	{
		return isPressed;
	}

	public void OnEnable()
	{
		//_collider = gameObject.GetComponent<Collider>();
	}

	//WARNING: Sometimes one edge (Enter or Exit) gets dropped, and w e should try to gracefully recover
	void OnCollisionEnter(Collision collision)
	{
		isPressed = true;
	}
	void OnCollisionExit(Collision collision) {
		isPressed = false;
	}
}
