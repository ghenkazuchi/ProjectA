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
		firstIndex = -1;
		ClearHighlights();
	}

	public void EndSwitch()
	{
		ClearHighlights();
		firstIndex = -1;
		Hide();
	}

	private void OnMemberClicked(int index, BattleUnit unit)
	{
		Debug.Log("MemberClicked: " + index);	
		if (BattleSystem.Instance.GetBattleState() != BattleState.PositionSwitch) return;
		if(firstIndex < 0)
		{
			firstIndex = index;
			ClearHighlights();
			partymemberUIComponents[index].SetSelected(true);
			return;
		}
		if(firstIndex == index)
		{
			firstIndex = -1;
			ClearHighlights();
			return;
		}
		int secondIndex = index;

		var a = BattleSystem.Instance.GetBattleUnitAt(firstIndex);
		var b = BattleSystem.Instance.GetBattleUnitAt(secondIndex);
		bool aEmpty = (a == null || a.character == null);
		bool bEmpty = (b == null || b.character == null);
		if (aEmpty && bEmpty)
		{
			firstIndex = -1;
			ClearHighlights();
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
