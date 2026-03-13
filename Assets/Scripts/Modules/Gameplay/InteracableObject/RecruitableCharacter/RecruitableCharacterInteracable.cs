using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitableCharacterInteracable : Interacable,IRecruitableCharacter
{
	[Header("Character Data")]
	private RecruitableCharacterTemplate characterTemplate;
	private RecruitableCharacterData characterData;
	private bool hasRolled = false;

	public void InitCharacterData()
	{
		RollCharacter();
	}

	/// <summary>
	/// Rolls a template from the current pool and permanently claims it.
	/// Called on first interaction. Once rolled, the template sticks forever.
	/// </summary>
	private void RollCharacter()
	{
		if (hasRolled) return;

		var availableCharacters = GamePoolManager.Instance.GetCurrentRecruitablePool();
		if (availableCharacters.Count > 0)
		{
			characterTemplate = availableCharacters[Random.Range(0, availableCharacters.Count)];
			characterData = new RecruitableCharacterData(characterTemplate, characterTemplate.GetRandomLevel());

			// Permanently claim this template so no other interactable can roll it
			GamePoolManager.Instance.ClaimTemplate(characterTemplate);
			hasRolled = true;
		}
		else
		{
			Debug.LogWarning("No recruitable characters available for current game progress.");
			Destroy(gameObject);
		}
	}

	public void Recruit()
	{
		if (characterTemplate != null)
		{
			GamePoolManager.Instance.MarkRecruited(characterTemplate);
		}
		Destroy(gameObject);
	}

	public override void TriggerInteraction()
	{
		// Roll the character on first interaction — sticks forever after
		RollCharacter();
		if (characterData != null)
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnRecruitEnter, new object[] { characterData, this }));
		}
	}
}
