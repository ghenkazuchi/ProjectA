using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsInfo : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI[] StatsTexts;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] PlayerCharacter currentCharacter;
	private bool isOpened = false;
	public void Awake()
	{
		Close();
	}

	public void SetUp(PlayerCharacter currentCharacter)
	{
		this.currentCharacter = currentCharacter;
		Stat[] stats = (Stat[])System.Enum.GetValues(typeof(Stat));
		for (int i  = 0; i < StatsTexts.Length; i++)
		{
			Stat stat = stats[i];
			string statValue = currentCharacter.GetFinalStat(stat).ToString();
			StatsTexts[i].text = $"{stat}: {statValue}";
		}
	}
	public void Toggle(PlayerCharacter character)
	{
		if (isOpened)
		{
			Close();
		}
		else
		{
			currentCharacter = character;
			SetUp(currentCharacter);
			Open();
		}
	}
	public void Open()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		isOpened = true;
	}
	public void Close()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		currentCharacter = null;
		isOpened = false;
	}
}
