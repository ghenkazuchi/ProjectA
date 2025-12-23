using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogBox : MonoBehaviour
{
	public TextMeshProUGUI dialogText;
	[SerializeField] GameObject actionSelector;
	[SerializeField] GameObject attackSelector;
	[SerializeField] CanvasGroup turnEntityInfo;
	public List<TextMeshProUGUI> actionTexts;
	public List<SkillUI> skillUI;
	[SerializeField] float minDisplayTimePerDialog = 1.0f;
	[SerializeField] float delayBetweenDialogs = 0.5f;
	private Queue<string> dialogQueue = new Queue<string>();
	private Coroutine dialogCoroutine;
	public bool IsDialogTyping { get; private set; } = false;

	[SerializeField] Color highlightedColor;
	[SerializeField] Color normalColor;
	public int letterPerSecond;
	public void SetDialog(string dialog)
	{
		dialogText.text = dialog;
	}

	private void ShowTurnEntityInfo()
	{
		turnEntityInfo.alpha = 1f;
		turnEntityInfo.interactable = true;
		turnEntityInfo.blocksRaycasts = true;

	}
	private void HideTurnEntityInfo()
	{
		turnEntityInfo.alpha = 0f;
		turnEntityInfo.interactable = false;
		turnEntityInfo.blocksRaycasts = false;
	}
	public IEnumerator TypeDialog(string dialog)
	{
		HideTurnEntityInfo();
		IsDialogTyping = true;
		dialogText.text = "";
		float totalTypingTime = (float)dialog.Length / letterPerSecond;
		string currentDisplayedText = "";

		foreach (var letter in dialog.ToCharArray())
		{
			currentDisplayedText += letter;
			dialogText.text = currentDisplayedText;
			yield return new WaitForSeconds(1f / letterPerSecond);
		}

		if (totalTypingTime < minDisplayTimePerDialog)
		{
			yield return new WaitForSeconds(minDisplayTimePerDialog - totalTypingTime);
		}

		IsDialogTyping = false;
		ShowTurnEntityInfo();
	}

	public void EnableDialogText(bool enabled)
	{
		dialogText.enabled = enabled;
	}
	public void EnableActionSelector(bool enabled)
	{
		actionSelector.SetActive(enabled);
	}
	public void EnableAttackSelector(bool enabled)
	{
		attackSelector.SetActive(enabled);
		if (enabled)
		{
			for (int i = 0; i < skillUI.Count; i++) 
			{
				skillUI[i].HighlightSkill(false);
			}
		}
	}

	public void SetAttackName(List<ActiveSkill> skillList)
	{
		for (int i = 0; i < skillUI.Count; i++) 
		{
			if(i< skillList.Count)
			{
				skillUI[i].SetUp(skillList[i].SkillData.skillName, skillList[i].element);
			}
			else
			{
				skillUI[i].SetUp("---",null);
			}
		}
	}
	public void UpdateActionSelection(int selectedAction)
	{
		for (int i = 0; i < actionTexts.Count; i++)
		{
			if (i == selectedAction)
			{
				actionTexts[i].color = highlightedColor;
			}
			else
			{
				actionTexts[i].color = normalColor;
			}
		}
	}
	public void UpdateSkillSelection(int selectedSkill)
	{
		for (int i = 0; i < skillUI.Count; i++)
		{
			if (i == selectedSkill)
			{
				skillUI[i].HighlightSkill(true);
			}
			else
			{
				skillUI[i].HighlightSkill(false);
			}
		}
	}
	public void ClearDialog()
	{
		dialogText.text = "";
		EnableDialogText(false);
	}
	public void EnqueueDialog(string dialog)
	{
		dialogQueue.Enqueue(dialog);
		if (dialogCoroutine == null)
		{
			dialogCoroutine = StartCoroutine(ProcessDialogQueue());
		}
	}
	private IEnumerator ProcessDialogQueue()
	{
		while (dialogQueue.Count > 0)
		{
			string dialog = dialogQueue.Dequeue();
			yield return TypeDialog(dialog);
			yield return new WaitForSeconds(delayBetweenDialogs); // Delay after each dialog
		}
		dialogCoroutine = null;
	}
}
