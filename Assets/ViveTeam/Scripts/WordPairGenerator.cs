using System.Collections.Generic;
using UnityEngine;

public class WordPairGenerator : MonoBehaviour {
	[SerializeField] private List<WordBank> _wordBanks;

	//quickly test in editor only - add the component to a parent, then click the wheel to the upper right of the component, and you'll see "Test word generation" as an option
	[ContextMenu("Test Word Generation")]
	private void TestWordGeneration() {
		var uniqueStringsOutOfTarget = new HashSet<string>();
		var targetNumberOfPairs = 100;
		for(var i = 0;i < targetNumberOfPairs;i++) {
			var panelName = GenerateWordPair(WordBank.PartOfSpeech.NOUN, WordBank.PartOfSpeech.ADVERB);
			//TODO: add "un-" to indicate disabling switches
			uniqueStringsOutOfTarget.Add(panelName);
			Debug.Log(panelName); //testing if these are funny
		}
		Debug.Log(uniqueStringsOutOfTarget.Count + " vs target number of pairs: " + targetNumberOfPairs);  //testing how frequent collisions are. 12% with a 18 noun and 14 adverb option
	}

	public string GetRandomTypeOfWord(WordBank.PartOfSpeech partOfSpeech) {
		var wordsOfThatPartOfSpeech = new List<WordBank.Word>();
		foreach(var wordBank in _wordBanks) {
			wordsOfThatPartOfSpeech.AddRange(wordBank.GetWordsByPartOfSpeech(partOfSpeech));
		}

		//use if testing how robust your word dicitionary is
		//Debug.LogFormat("Part of speech {0} count {1}",partOfSpeech,wordsOfThatPartOfSpeech.Count);

		//NOTE: Failing fast is usually best, but that would mean checking my data more thoroughly than makes sense here

		return wordsOfThatPartOfSpeech.Count == 0
			? "null"
			: wordsOfThatPartOfSpeech[Random.Range(0, wordsOfThatPartOfSpeech.Count)].word;
	}

	public string GenerateWordPair(WordBank.PartOfSpeech firstWordPartOfSpeech, WordBank.PartOfSpeech secondPartOfSpeech) {
		return string.Format("{0} {1}", GetRandomTypeOfWord(firstWordPartOfSpeech), GetRandomTypeOfWord(secondPartOfSpeech));
	}

	public List<string> getUniqueListOfWordPairsThisLong(int targetLength) {
		var uniqueStringsOutOfTarget = new HashSet<string>();
		int maxTriesToGetTargetPairs = (int)Mathf.Pow(2, 14);
		for(var i = 0;i < maxTriesToGetTargetPairs;i++) {
			var panelName = GenerateWordPair(WordBank.PartOfSpeech.NOUN, WordBank.PartOfSpeech.ADVERB);
			//TODO: add "un-" to indicate disabling switches
			uniqueStringsOutOfTarget.Add(panelName);
			if(uniqueStringsOutOfTarget.Count == targetLength) {
				break;
			}
		}
		return new List<string>(uniqueStringsOutOfTarget);
	}
}