using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitUIController : MonoBehaviour
{
	[Header("Basic info")]
	[SerializeField] TextMeshProUGUI recruitmentCost;
	[SerializeField] Image characterPortrait;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI raceText;
	[SerializeField] TextMeshProUGUI ElementText;
	[SerializeField] TextMeshProUGUI[] traitTexts;

	[Header("Skills Display")]
	[SerializeField] ScrollRect skillScrollRect;
	[SerializeField] Transform skillsContent;
	[SerializeField] GameObject skillDisplayPrefab;

	[Header("Button")]
	[SerializeField] Button closeButton;
	[SerializeField] Button detailButton;
	[SerializeField] Button showActiveSkillsButton;
	[SerializeField] Button showPassiveSkillsButton;
	[SerializeField] Button showAuraSkillsButton;
	[SerializeField] Button recruitButton;
	[Header("CanvasGroup")]
	[SerializeField] CanvasGroup traitsDetailCanvasGroup;
	private bool isStatsOpen = false;

	private void Start()
	{
		recruitButton.onClick.AddListener(OnRecruitButtonClicked);
		closeButton.onClick.AddListener(() => CloseRecruitUI());
		detailButton.onClick.AddListener(() => ToggleStatsUI());
		showActiveSkillsButton.onClick.AddListener(() => DisplayCharacterSkills(currentRecruitableCharacterData, SkillType.Active));
		showPassiveSkillsButton.onClick.AddListener(() => DisplayCharacterSkills(currentRecruitableCharacterData, SkillType.Passive));
		traitsDetailCanvasGroup.alpha = 0f;
		traitsDetailCanvasGroup.blocksRaycasts = false;
		traitsDetailCanvasGroup.interactable = false;
		SetStatsUI(false);
	}

	private RecruitableCharacterData currentRecruitableCharacterData;
	private IRecruitableCharacter currentRecruitableCharacterInteractive;
	public void ShowRecruitUI(RecruitableCharacterData characterData, IRecruitableCharacter recruitableCharacterInteractive)
	{
		currentRecruitableCharacterData = characterData;
		currentRecruitableCharacterInteractive = recruitableCharacterInteractive;
		DisplayCharacterInfo(characterData);
		DisplayCharacterTraits(characterData);
		ClearContainer(skillsContent);
		DisplayCharacterSkills(characterData, SkillType.Active);
	}
	private void OnRecruitButtonClicked()
	{
		if (currentRecruitableCharacterData == null) return;

		int cost = currentRecruitableCharacterData.recruitmentCost;
		if (DataManager.Instance != null && DataManager.Instance.Currency != null)
		{
			if (!DataManager.Instance.Currency.TrySpend(CurrencyType.SoulDusk, cost))
			{
				Debug.LogWarning($"Not enough Souldusk to recruit {currentRecruitableCharacterData.characterName}. Need {cost} Souldusk.");
				// Optionally pop a Toast UI here
				return;
			}
		}

		Debug.Log("Recruited");
		MessageManager.Instance.SendMessage(new Message(MessageType.OnRecruitCharacter, new object[] { currentRecruitableCharacterData, currentRecruitableCharacterInteractive }));
	}
	private void CloseRecruitUI()
	{
		SetStatsUI(false);
		MessageManager.Instance.SendMessage(new Message(MessageType.OnRecruitCharacterUIClose));
	}

	private void ToggleStatsUI()
	{
		SetStatsUI(!isStatsOpen);
	}

	private void SetStatsUI(bool open)
	{
		isStatsOpen = open;
		traitsDetailCanvasGroup.alpha = open ? 1f : 0f;
		traitsDetailCanvasGroup.interactable = open;
		traitsDetailCanvasGroup.blocksRaycasts = open;
	}
	private void DisplayCharacterInfo(RecruitableCharacterData characterData)
	{
		characterPortrait.sprite = characterData.characterTemplate.entityData.EntityPortrait;
		nameText.text = characterData.characterTemplate.entityData.EntityName;
		levelText.text = $"Level: {characterData.level}";
		raceText.text = $"Race: {characterData.characterTemplate.raceData.raceType.ToString()}";
		ElementText.text = $"Element: {characterData.characterTemplate.entityData.EntityElement.ToString()}";

		if (recruitmentCost != null)
		{
			recruitmentCost.text = $"{characterData.recruitmentCost}";
		}
	}

	private void DisplayCharacterSkills(RecruitableCharacterData characterData,SkillType skillType)
	{
		ClearContainer(skillsContent);
		List<BaseSkillData> skillsToDisplay = new List<BaseSkillData>();
		switch (skillType)
		{
			case SkillType.Active:
				foreach (var skill in characterData.availableActiveSkills)
				{
					skillsToDisplay.Add(skill);
				}
				break;
			case SkillType.Passive:
				foreach (var skill in characterData.availablePassiveSkills)
				{
					skillsToDisplay.Add(skill);
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
		foreach(Transform child in container)
		{
			Destroy(child.gameObject);
		}
	}
	private void DisplayCharacterTraits(RecruitableCharacterData characterData)
	{
		Trait[] traits = (Trait[])System.Enum.GetValues(typeof(Trait));
		for (int i = 0; i < traitTexts.Length; i++) 
		{
			Trait trait = traits[i];
			int baseValue = characterData.characterTemplate.entityData.BaseTraits.ContainsKey(trait) ? characterData.characterTemplate.entityData.BaseTraits[trait] : 0;
			traitTexts[i].text = $"{trait}: {baseValue}";
		} 
	}
}
