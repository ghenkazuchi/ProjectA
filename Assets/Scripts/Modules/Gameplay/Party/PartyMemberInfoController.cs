using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberInfoController : Singleton<PartyMemberInfoController>, IMessageHandle
{
	[SerializeField] PlayerCharacter currentCharacter;
	[SerializeField] Image portrait;
	[SerializeField] TextMeshProUGUI characterNameText;
	[SerializeField] TextMeshProUGUI characterLevelText;
	[SerializeField] TextMeshProUGUI raceText;
	[SerializeField] TextMeshProUGUI classText;
	[SerializeField] TextMeshProUGUI expText;
	[SerializeField] CanvasGroup canvasGroup;
	[Header("Skills Display")]
	[SerializeField] ScrollRect skillsScrollRect;
	[SerializeField] Transform skillsContent;
	[SerializeField] GameObject skillDisplayPrefab;
	[Header("Buttons")]	
	[SerializeField] Button showActiveSkillButton;
	[SerializeField] Button showPassiveSkillButton;
	[SerializeField] Button showAuraSkillButton;
	[SerializeField] Button showEquipmentButton;
	[SerializeField] Button showStatsButton;
	[SerializeField] Button closeButton;
	[Header("Stats And Equipment Canvas")]
	[SerializeField] StatsInfo statsInfo;
	[SerializeField] EquipmentInfo equipmentInfo;
	private void Awake()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		showActiveSkillButton.onClick.AddListener(() => DisplayCharacterSkills(currentCharacter, SkillType.Active));
		showPassiveSkillButton.onClick.AddListener(() => DisplayCharacterSkills(currentCharacter, SkillType.Passive));
		closeButton.onClick.AddListener(() =>
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			currentCharacter = null;
			ClearContainer(skillsContent);
			MessageManager.Instance.SendMessage(new Message(MessageType.OnPartyMemberInfoClose));
		});
		showEquipmentButton.onClick.AddListener(() => DisplayCharacterEquipment(currentCharacter));
		showStatsButton.onClick.AddListener(() => DisplayCharacterStats());
	}

	private void DisplayCharacterEquipment(PlayerCharacter character)
	{
		equipmentInfo.Show(character);
	}

	private void DisplayCharacterStats()
	{
		statsInfo.Show(currentCharacter);
	}
	private void DisplayCharacterSkills(PlayerCharacter character, SkillType skillType)
	{
		ClearContainer(skillsContent);
		List<BaseSkillData> skillsToDisplay = new List<BaseSkillData>();
		switch (skillType)
		{
			case SkillType.Active:
				foreach (var skill in currentCharacter.usableSkills)
				{
					skillsToDisplay.Add(skill.SkillData);
				}
				break;
			case SkillType.Passive:
				foreach (var skill in currentCharacter.activePassiveSkills)
				{
					skillsToDisplay.Add(skill.PassiveSkillData);
				}
				break;
		}
		foreach (var skillData in skillsToDisplay)
		{
			GameObject skillObj = Instantiate(skillDisplayPrefab, skillsContent);
			SkillDisplayUI skillDisplay = skillObj.GetComponent<SkillDisplayUI>();
			skillDisplay.SetupSkillName(skillData);
		}
	}
	private void ClearContainer(Transform container)
	{
		foreach (Transform child in container)
		{
			Destroy(child.gameObject);
		}
	}
	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnPartyMemberInfoUpdate, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMemberInfoUpdate, this);
	}
	public void Handle(Message message)
	{
		switch(message.type)
		{
			case MessageType.OnPartyMemberInfoUpdate:
				currentCharacter = message.data[0] as PlayerCharacter;
				EnableCanvasGroup();
				SetData();
			break;
		}
	}
	private void EnableCanvasGroup()
	{
		ClearContainer(skillsContent);
		DisplayCharacterSkills(currentCharacter, SkillType.Active);
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}
	private void SetData()
	{
		if (currentCharacter != null) 
		{
			portrait.sprite = currentCharacter.entityData.EntitySprite;
			characterNameText.text = currentCharacter.entityData.EntityName;
			characterLevelText.text = $"Level: {currentCharacter.level}";
			raceText.text =  $"Race: {currentCharacter.RaceDataName}";	
			classText.text = $"Class: {currentCharacter.GetClassData.classType}";
			expText.text = $"EXP: {currentCharacter.CurrentExp} / {currentCharacter.GetExpNeededForNextLevel()}";
		}
	}

}
