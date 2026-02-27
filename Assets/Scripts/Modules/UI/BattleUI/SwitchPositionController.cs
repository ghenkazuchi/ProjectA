using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPositionController : MonoBehaviour, IMessageHandle
{
	[SerializeField] private List<PartymemberUIComponent> partymemberUIComponents;
	[SerializeField] private CanvasGroup canvasGroup;

	private int firstIndex = -1;

	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnBattleStart, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleStart, this);
	}
	private void Awake()
	{
		Hide();
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnBattleStart:
				Init();
				break;
		}
	}

	private void Init()
	{
		Debug.Log("[SwitchPosition] Init()", this);
		firstIndex = -1;
		for(int i = 0;i< partymemberUIComponents.Count; i++)
		{
			var ui = partymemberUIComponents[i];
			if (ui == null) continue;

			ui.Bind(i, OnMemberClicked);
		}
	}

	public void BeginSwitch()
	{
		for (int i = 0; i < partymemberUIComponents.Count; i++)
		{
			if (partymemberUIComponents[i] != null)
				partymemberUIComponents[i].Refresh();
		}

		Show();
		ClearHighlights();

		// Auto-lock firstIndex to the current turn entity's position
		firstIndex = -1;
		var currentEntity = BattleSystem.Instance.currentTurnEntity;

		if (currentEntity != null)
		{
			int searchCount = Mathf.Min(partymemberUIComponents.Count, 6);
			for (int i = 0; i < searchCount; i++)
			{
				var unit = BattleSystem.Instance.GetBattleUnitAt(i);
				if (unit != null && unit.character == currentEntity)
				{
					firstIndex = i;
					if (partymemberUIComponents[i] != null)
						partymemberUIComponents[i].SetSelected(true);
					break;
				}
			}
		}
	}

	public void EndSwitch()
	{
		ClearHighlights();
		firstIndex = -1;
		Hide();
	}

	private void OnMemberClicked(int index, BattleUnit unit)
	{
		if (BattleSystem.Instance.GetBattleState() != BattleState.PositionSwitch) return;

		// firstIndex is already locked to current turn entity
		if (firstIndex < 0) return;

		// Clicking the same slot = do nothing
		if (firstIndex == index) return;

		int secondIndex = index;

		// Allow swapping with empty slots — only block if BOTH are empty
		var a = BattleSystem.Instance.GetBattleUnitAt(firstIndex);
		var b = BattleSystem.Instance.GetBattleUnitAt(secondIndex);
		bool aEmpty = (a == null || a.character == null);
		bool bEmpty = (b == null || b.character == null);
		if (aEmpty && bEmpty)
		{
			return;
		}

		BattleSystem.Instance.ExecuteSwitchPosition(firstIndex, secondIndex);

		EndSwitch();
	}
	private void ClearHighlights()
	{
		for(int i = 0;i < partymemberUIComponents.Count; i++)
		{
			if(partymemberUIComponents[i] != null)
				partymemberUIComponents[i].SetSelected(false);
		}
	}
	public void Show()
	{
		canvasGroup.alpha = 1;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
}
