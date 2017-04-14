using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LabelledButton : MonoBehaviour
{
	[SerializeField] private TMP_Text textField;
	public void SetText(string text)
	{
		textField.text = text;
	}
	public string GetText()
	{
		return textField.text;
	}
}
