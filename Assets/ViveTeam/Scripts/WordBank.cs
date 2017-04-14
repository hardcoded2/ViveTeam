using System;
using System.Collections.Generic;
using UnityEngine;

public class WordBank : MonoBehaviour
{
	public enum PartOfSpeech
	{
		NOUN,
		VERB,
		ADJECTIVE,
		ADVERB 
	}

	public List<Word> wordsToUse;

	[Serializable]
	public class Word
	{
		public PartOfSpeech partOfSpeech;
		public string word;
		//TODO: multiple parts of speech from one form of a word, ie adjective to verb forms captured here - beautiful(adj), beautiflly
	}

	public List<Word> GetWordsByPartOfSpeech(PartOfSpeech partOfSpeech)
	{
		List<Word> wordsOfTargetPartOfSpeech = new List<Word>();
		foreach (var word in wordsToUse)
		{
			if (word.partOfSpeech == partOfSpeech)
			{
				wordsOfTargetPartOfSpeech.Add(word);
			}
		}
		return wordsOfTargetPartOfSpeech;
	}
}