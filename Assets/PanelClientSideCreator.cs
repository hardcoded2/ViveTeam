using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PanelClientSideCreator : NetworkBehaviour
{
	[SyncVar] public int numMissedCommands; //server will poll this to see when failure happens
	[SerializeField] private List<LabelledButton> buttons;

	public void TEST_ONLY_SetButtonsForThisClient(string[] buttonNames)
	{
		if(buttonNames.Length != buttons.Count) {
			Debug.LogError("Different number of button names sent than we have buttons locally, not good");
		}
		for(int i = 0;i < buttons.Count;i++) {
			var labelledButton = buttons[i];
			labelledButton.SetText(buttonNames[i]);
		}
	}
	//NOTE: clientrpcs must have function names with "Rpc", and commands to server must start with "Cmd", and List<string> is not supported
	[ClientRpc]
	public void RpcSetButtonsForThisClient(string[] buttonNames)
	{
		TEST_ONLY_SetButtonsForThisClient(buttonNames); //re-use functionality that is tested, and still maintain the unet contract
	}

	[ClientRpc]
	public void RpcSetCommandAndTimeoutForThisClient(string commandForThisClient) {

	}

	[ClientRpc]
	public void RpcGameStarted()
	{
		
	}

	[ClientRpc]
	public void RpcGameEnded() {

	}

	[ClientRpc]
	public void RpcTookDamage() { //increase particle effects, blackout on vive/etc
		
	}

}
