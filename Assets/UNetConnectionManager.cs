using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UNetConnectionManager : MonoBehaviour
{
	private NetworkClient myClient;

	public void Start()
	{
		StartCoroutine(SetNetworkingMode());
	}
	private IEnumerator SetNetworkingMode()
	{
		while (true)
		{
			if(Input.GetKeyDown(KeyCode.S)) {
				SetupServer();
				yield break;
			}

			if(Input.GetKeyDown(KeyCode.C)) {
				SetupClient();
				yield break;
			}

			if(Input.GetKeyDown(KeyCode.B)) {
				SetupServer();
				SetupLocalClient();
				yield break;
			}
			yield return null;
		}
	}

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(4444);
	}

	// Create a client and connect to the server port
	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);
		myClient.Connect("127.0.0.1", 4444);
	}

	// Create a local client and connect to the local server
	public void SetupLocalClient()
	{
		myClient = ClientScene.ConnectLocalServer();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);
	}

	// client function
	public void OnConnected(NetworkMessage netMsg)
	{
		Debug.Log("Connected to server");
	}
}