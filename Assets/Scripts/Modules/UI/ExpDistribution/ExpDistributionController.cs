using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class ExpDistributionController : MonoBehaviour
{
	[SerializeField] PlayerParty playerParty;
	[SerializeField] CanvasGroup expDistributionCanvasGroup;
	[SerializeField] List<PartyMemberExpInfo> partyMemberExpInfos;
	[SerializeField] Button closeButton;
	[SerializeField] private BattleSystem battleSystem;
	private readonly Dictionary<GridPosition, int> positionToPartyMemberDisplay = new Dictionary<GridPosition, int>
	{
		{ new GridPosition(0, 0), 0 },
		{ new GridPosition(1, 0), 3 },
		{ new GridPosition(1, 2), 5 },
		{ new GridPosition(0, 1), 1 },
		{ new GridPosition(1, 1), 4 },
		{ new GridPosition(0, 2), 2 },
	};
	private void Awake()
	{
		SetUpListeners();
	}
	private void InitializeUI()
	{
		Debug.Log("=== Initializing Exp Distribution UI ===");
		for (int i = 0; i < partyMemberExpInfos.Count; i++)
		{
			if (partyMemberExpInfos[i] != null)
			{
				GridPosition correspondingPosition = GetGridPositionForUIIndex(i);

				EntityBase entity = playerParty.GetEntityAtPosition(correspondingPosition);
				partyMemberExpInfos[i].SetUp(entity, correspondingPosition, this);
			}
			else
			{
				Debug.LogWarning($"PartyMemberExpInfo at index {i} is null!");
			}
		}
	}
	private GridPosition GetGridPositionForUIIndex(int uiIndex)
	{
		foreach (var kvp in positionToPartyMemberDisplay)
		{
			if (kvp.Value == uiIndex)
			{
				return kvp.Key;
			}
		}
		Debug.LogError($"No GridPosition found for UI index {uiIndex}");
		return new GridPosition(-1, -1);
	}
	private void SetUpListeners()
	{
		if (closeButton != null)
			closeButton.onClick.AddListener(CloseExpDistributionPopup);
	}

	private void CloseExpDistributionPopup()
	{
		Debug.Log("Closing Exp Distribution Popup");
		expDistributionCanvasGroup.alpha = 0f;
		expDistributionCanvasGroup.blocksRaycasts = false;
		expDistributionCanvasGroup.interactable = false;
		battleSystem.HandleAfterMatch();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleOver));
	}
	public void ShowExpDistribution(List<int> expGainedPerMember)
	{
		InitializeUI();
		int expIndex = 0;
		for (int i = 0; i < partyMemberExpInfos.Count; i++)
		{
			var info = partyMemberExpInfos[i];
			if (info != null && info.GetCharacterData() != null && expIndex < expGainedPerMember.Count)
			{
				info.AnimateExpGain(expGainedPerMember[expIndex]);
				expIndex++;
			}
		}
		expDistributionCanvasGroup.alpha = 1;
		expDistributionCanvasGroup.interactable = true;
		expDistributionCanvasGroup.blocksRaycasts = true;
	}
}