using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnvilPopup : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;

	[SerializeField] private AnvilChooseCharacter chooseCharacterPanel;
	[SerializeField] private AnvilCharacterItem currentCharacterItemPanel;
	[SerializeField] private AnvilUpgradeItem upgradeItemPanel;
	[SerializeField] private AnvilAnimation anvilAnimation;

	private PlayerCharacter currentPlayerCharacter;
	private void Awake()
	{
		HideImmediate();
	}
	public void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		chooseCharacterPanel.Show(OnCharacterPicked);
		upgradeItemPanel.Close();
	}

	public void Hide()
	{
		HideImmediate();
		currentPlayerCharacter = null;
	}

	private void HideImmediate()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		chooseCharacterPanel.Close();
		upgradeItemPanel.Close();
		currentCharacterItemPanel.Close();
	}

	private void OnCharacterPicked(PlayerCharacter pc)
	{
		currentPlayerCharacter = pc;
		currentCharacterItemPanel.Show(pc, OnItemToggle);

		upgradeItemPanel.Init(pc, onCombined: (resultItem) =>
		   {
			currentCharacterItemPanel.Refresh();
			       if (resultItem != null && anvilAnimation != null)
				       {
				   currentCharacterItemPanel.Close();
				   upgradeItemPanel.Close();
				   anvilAnimation.SetResult(resultItem);
				anvilAnimation.PlayAnimation();
			   }
		   });
		upgradeItemPanel.Show();
	}
	private void OnItemToggle(int index, Item item, bool isSelected)
	{
		if(currentPlayerCharacter == null || item == null) return;
		if (isSelected)
		{
			bool accepted = upgradeItemPanel.TrySelect(index, item);
			if (!accepted)
			{
				currentCharacterItemPanel.SetItemSelectedVisual(index, false);
			}
		}
		else
		{
			upgradeItemPanel.RemoveSelection(index);
		}
	}
}
