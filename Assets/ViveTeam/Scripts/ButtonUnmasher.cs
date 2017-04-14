using UnityEngine;

//NOTE: this is designed to work only on the y axis to make the code easier to read
public class ButtonUnmasher : MonoBehaviour
{
	private Transform _buttonTransform;
	public VTButton button;
	public Transform pressedButtonPosition;
	public Transform restingButtonPostion;
	float maxDistance;
	float snapDistance;
	//public AnimationCurve UnpressCurve; //need to know if the button is being pressed to do this, evaluates from time zero to one
	private void OnEnable()
	{
		 maxDistance = Vector3.Distance(restingButtonPostion.position, pressedButtonPosition.position);
		 snapDistance = maxDistance / 10f;
		_buttonTransform = button.transform;
		_buttonTransform.transform.position = restingButtonPostion.position;
			//WARNING: this can fight with the restrict movment if not careful
	}
	public bool considerButtonPressed()
	{
		return Vector3.Distance(_buttonTransform.position, pressedButtonPosition.position) < snapDistance*2;
	}
	public void Update()
	{
		var buttonPos = _buttonTransform.position;
		var snapped = false;

		//handle invalid movements from physics
		if ( buttonPos.y < pressedButtonPosition.position.y)
		{
			buttonPos = pressedButtonPosition.position;
			snapped = true;
		}
		if (buttonPos.y > restingButtonPostion.position.y)
		{
			buttonPos = restingButtonPostion.position;
			snapped = true;
		}
		
		if (!snapped && !button.IsPresssed())
		{
			//we always float up for now (not doing this while pressed is better)
			//float normalizedInstintaniousForceTowardsOrigin = Mathf.Lerp(0, maxDistance, distanceFromRestingPosition);
			var normalizedInstintaniousForceTowardsOrigin = maxDistance/100f/maxDistance; //should slide back over 100 frames
																						  //var lerpAmount = Vector3.Lerp(buttonPos, restingButtonPostion.position, normalizedInstintaniousForceTowardsOrigin);
																						  //buttonPos += lerpAmount;
			buttonPos = Vector3.MoveTowards(buttonPos, restingButtonPostion.position, normalizedInstintaniousForceTowardsOrigin);
		}

		_buttonTransform.position = buttonPos;
	}
}