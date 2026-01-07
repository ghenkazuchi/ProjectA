using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeAllFont : MonoBehaviour
{
	public TMP_FontAsset newFont;

	void Start()
	{
		TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
		foreach (var t in texts)
		{
			t.font = newFont;
		}
	}
}
