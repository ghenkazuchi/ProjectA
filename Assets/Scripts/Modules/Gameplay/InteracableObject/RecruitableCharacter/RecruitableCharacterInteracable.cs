using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitableCharacterInteracable : Interacable,IRecruitableCharacter
{
	[Header("Character Data")]
	[SerializeField] RecruitableCharacterTemplate characterTemplate;
	[SerializeField] int assignedLevel;

	private RecruitableCharacterData characterData;
	private void Start()
	{
		InitCharacterData();
	}
	public void InitCharacterData()
	{

		var availableCharacters = GamePoolManager.Instance.GetCurrentRecruitablePool();
		if (availableCharacters.Count > 0)
		{
			characterTemplate = availableCharacters[Random.Range(0, availableCharacters.Count)];
			characterData = new RecruitableCharacterData(characterTemplate, characterTemplate.GetRandomLevel());
		}
		else
		{
			Debug.LogWarning("No recruitable characters available for current game progress.");
			Destroy(gameObject);
		}
	}
	public void Recruit()
	{
		Destroy(gameObject);
	}


	public override void TriggerInteraction()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnRecruitEnter, new object[] { characterData, this }));
	}
}
