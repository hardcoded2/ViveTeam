using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class PanelClientSideCreator : NetworkBehaviour
{
	[SerializeField] private List<LabelledButton> _buttons;
	[SerializeField] private NetworkedPanelCreator _wordCreator;
	public Action<string> OnClientClickedOnButon;
	[SyncVar]
	public int ShipHealth; //When zero, the ship dies 

	private List<string> _myButtonNames;

	[Serializable]
	public class CommandForThisClient //or does unet only serialize arbitrary structs?
	{
		public string ButtonToShowThatWeClicked;
		public string ButtonThatWeShouldHitNext;
		public float TimeoutTime;
	}

	public void SetButtonsNonRPC(string[] buttonNames)
	{
		if(buttonNames.Length != _buttons.Count) {
			Debug.LogError("Different number of button names sent than we have buttons locally, not good");
		}
		for(int i = 0;i < _buttons.Count;i++) {
			var labelledButton = _buttons[i];
			labelledButton.SetText(buttonNames[i]);
		}
		_myButtonNames = new List<string>(buttonNames);
	}

	private int _myClientID = -1;

	[TargetRpc]
	public void TargetSetButtonsForThisClient(NetworkConnection targetIgnoreMe, int myClientIdFromHost, string[] buttonNames)
	{
		_myClientID = myClientIdFromHost;
		SetButtonsNonRPC(buttonNames); //re-use functionality that is tested, and still maintain the unet contract
	}

	[ClientRpc]
	public void RpcGameStarted(string[] buttonsToAskFor,string[] buttonsToPress)
	{
		
	}

	[ClientRpc]
	public void RpcGameEnded() {

	}

	//in order for the client to calculate this
	[ClientRpc]
	public void RpcTookDamage() { //increase particle effects, blackout on vive/etc
		
	}

	[Command]
	public void CmdTellServerWeDidSomething(string buttonClickedOn)
	{
		if(OnClientClickedOnButon != null)
		{
			OnClientClickedOnButon(buttonClickedOn);   //manager checks if this was in the list of active commands and lets us know if we should consider failure or success
		}
	}

	public void PanelManagerToldUsIfClientClickedRightButton(bool wasRightButton)
	{
		CmdClientToldUsIfSuccessOrFailure(wasRightButton);
	}

	[Command]
	public void CmdClientToldUsIfSuccessOrFailure(bool clientHitCorrectButton)
	{
		if(clientHitCorrectButton)
		{
			ShipHealth--;
			if(ShipHealth < 0)
			{
				ShipHealth = 0;
			}
			RpcTookDamage(); //trigger instant damage effects
		} else
		{
			ShipHealth++;
		}
		if(ShipHealth == 0)
		{
			RpcGameEnded();
		}
		//TODO: adjust smoke/havoc based on totla number of missed commands now
	}

	//TODO: find out what is the exact trigger for when another client joins
	public void Pregame()
	{
		
	}

	private Dictionary<int, List<string>> _clientConnectionIdToWordsTheyOwn;

	[Command]
	public void CmdTellServerWeGotWords()
	{
		_clientsAcknowledgedSetup++;
	}

	private int _clientsAcknowledgedSetup = 0;
	private IEnumerator HostGameLoop()
	{
		int numberOfClients = 2;
		int wordsPerClient = _buttons.Count;
		int numberOfWordsTotalToRequest = numberOfClients * wordsPerClient;
		var allWords = _wordCreator.GetWordPairs(numberOfWordsTotalToRequest); //ask all at once so that they're unique
		_clientConnectionIdToWordsTheyOwn = new Dictionary<int, List<string>>();
		Dictionary<int,NetworkConnection> connectedClients = new Dictionary<int, NetworkConnection>();
		int connectionId = 0;
		foreach (var networkConnection in NetworkServer.connections)
		{
			if(networkConnection == null)
				continue;
			if(!networkConnection.isConnected && !networkConnection.isReady)
				continue;
			connectedClients.Add(connectionId, networkConnection);
			connectionId++;
		}

		for (int i = 0; i < numberOfClients; i++)
		{
			int firstWordForThisClient = i*wordsPerClient;
			List<string> portionOfListForThisClient = allWords.GetRange(firstWordForThisClient,
				firstWordForThisClient + wordsPerClient);
			_clientConnectionIdToWordsTheyOwn.Add(i,portionOfListForThisClient);
			RpcSetButtonsForThisClient(connectedClients[i], i, portionOfListForThisClient.ToArray());
		}

		while (_clientsAcknowledgedSetup < numberOfClients)
		{
			yield return null;
		}

		RpcGameStarted(randomList(allWords, numberOfClients), randomList(allWords, numberOfClients));
	}

	string[] randomList(List<string> fullList, int count)
	{
		string[] toReturn = new string[count];
		for (int i = 0; i < count; i++)
		{
			toReturn[i] = fullList[Random.Range(0,fullList.Count)];
		}
		return toReturn;
	} 

}
