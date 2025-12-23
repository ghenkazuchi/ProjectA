using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AnvilChooseCharacter : MonoBehaviour
{
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Button closeButton;
	[SerializeField] Transform characterContainer;
	[SerializeField] AnvilCharacterSelectionUIComponent characterButtonPrefabs;
	private Action<PlayerCharacter> onChosen;

	private void Awake()
	{
		Hide();
		closeButton.onClick.AddListener(ExitAnvilPopup);
	}

	public void Close()
	{
		Hide();
		ClearList();
		onChosen = null;
	}

	public void ExitAnvilPopup()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnAnvilPopupClose));
	}
	public void Show(Action<PlayerCharacter> onPick)
	{
		onChosen = onPick;
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		BuildList();
	}
	public void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
	private void BuildList()
	{
		ClearList();
		var playerParty = GameController.Instance.GetPlayerParty();
		var list = playerParty.GetAllPlayerCharacter();
		foreach (var character in list)
		{
			var go = Instantiate(characterButtonPrefabs.gameObject, characterContainer);
			var comp = go.GetComponent<AnvilCharacterSelectionUIComponent>();
			comp.SetUp(character, (pc) =>
			{
				onChosen?.Invoke(pc);
				Close();
			});

		}
	}
	private void ClearList()
	{

		foreach (Transform child in characterContainer)
		{
			Destroy(child.gameObject);
		}
	}
}
