using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictMovement : MonoBehaviour //if I don't want rigidbody constraints, I have to add them myself
{
	public bool allowLocalXMovment;
	public bool allowLocalYMovment;
	public bool allowLocalZMovment;

	private Vector3 _originalPosition;
	private Transform _transformCached;

	public void OnEnable()
	{
		_transformCached = transform;
		_originalPosition = _transformCached.localPosition;
	}

	// Update is called once per frame
	void Update ()
	{
		var transformLocalPosition = _transformCached.position;
		var pos = _transformCached.position;
		bool changedPos = false;
		if (!allowLocalXMovment)
		{
			if(!Mathf.Approximately(_originalPosition.x, pos.x))
			{
				pos.x = _originalPosition.x;
				changedPos = true;
			}
		}
		if(!allowLocalYMovment) {
			if(!Mathf.Approximately(_originalPosition.y, pos.y)) {
				pos.y = _originalPosition.y;
				changedPos = true;
			}
		}
		if(!allowLocalZMovment) {
			if(!Mathf.Approximately(_originalPosition.z, pos.z)) {
				pos.z = _originalPosition.z;
				changedPos = true;
			}
		}
		if (changedPos)
		{
			//we have to re-set this as the changes we made were local in scope only since the "Postion" from unity is a struct, which has slightly different calling conventions than classes 
			//Treat position like it is a value, like you locally set an "int" value that you got from the class originally
			_transformCached.position = pos;
		}
	}
}
