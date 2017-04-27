using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPanelCreator : MonoBehaviour {
	//TODO: only do as host
	[SerializeField] private WordPairGenerator _wordPairGenerator;

	//Call once per game to get all pairs and divide them among clinets 
	public List<string> GetWordPairs(int totalUniqueWordPairs)
	{
		return _wordPairGenerator.getUniqueListOfWordPairsThisLong(totalUniqueWordPairs);
	}

	[ContextMenu("Test setting word pairs on local client prefab")]
	private void SetLocalPairsForDebugging()
	{
		FindObjectOfType<PanelClientSideCreator>().SetButtonsNonRPC(GetWordPairs(9).ToArray());
	}

}
