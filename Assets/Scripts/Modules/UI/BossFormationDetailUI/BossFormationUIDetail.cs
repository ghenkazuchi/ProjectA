using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossFormationUIDetail : MonoBehaviour
{
	[Header("Current Monster Info")]
	[SerializeField] private TextMeshProUGUI currentMonsterName;
	[SerializeField] private TextMeshProUGUI currentMonsterLevel;
	[SerializeField] private TextMeshProUGUI currentMonsterType;
	[SerializeField] private TextMeshProUGUI currentMonsterRank;
	[Header("Current Monster Stat")]
	[SerializeField] private List<TextMeshProUGUI> currentMonsterTraitStats;
	[Header("Current Monster Skill Container")]
	[SerializeField] private Transform currentMonsterSkillContainer;
	[Header("Skill List Switch Buttons")]
	[SerializeField] private Button activeSkillButton;
	[SerializeField] private Button passiveSkillButton;

	[SerializeField] private TextMeshProUGUI skillTextPrefab;

	[SerializeField] private CanvasGroup canvasGroup;

	private MonsterCharacter currentDisplayedMonster;

	private void Awake()
	{
		activeSkillButton.onClick.AddListener(ShowActiveSkills);
		passiveSkillButton.onClick.AddListener(ShowPassiveSkills);
		Hide();
	}

	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		currentDisplayedMonster = null;
	}

	public void Show(Member member)
	{
		canvasGroup.alpha = 1;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

		if (member != null && member.monster != null)
		{
			currentMonsterName.text = member.monster.EntityName;
			currentMonsterLevel.text = "Lv." + member.level;

			if (member.race != null)
				currentMonsterType.text = member.race.type.ToString();
			else
				currentMonsterType.text = "";

			if (member.rank != null)
				currentMonsterRank.text = member.rank.monsterRank.ToString();
			else
				currentMonsterRank.text = "";

			currentDisplayedMonster = new MonsterCharacter(member.rank, member.race, member.level, member.monster);
			currentDisplayedMonster.InitializeEntity(member.level);

			Trait[] traits = (Trait[])System.Enum.GetValues(typeof(Trait));
			for (int i = 0; i < currentMonsterTraitStats.Count && i < traits.Length; i++)
			{
				Trait trait = traits[i];
				int statValue = currentDisplayedMonster.GetCurrentTrait(trait);
				currentMonsterTraitStats[i].text = $"{trait}: {statValue}";
			}

			ShowActiveSkills();
		}
	}

	private void ShowActiveSkills()
	{
		ClearSkillContainer();
		if (currentDisplayedMonster == null) return;

		foreach (var activeSkill in currentDisplayedMonster.GetUsableSkills())
		{
			if (activeSkill != null && activeSkill.SkillData != null)
			{
				var textUI = Instantiate(skillTextPrefab, currentMonsterSkillContainer);
				textUI.text = activeSkill.SkillData.skillName;
			}
		}
	}

	private void ShowPassiveSkills()
	{
		ClearSkillContainer();
		if (currentDisplayedMonster == null) return;

		foreach (var passiveSkill in currentDisplayedMonster.activePassiveSkills)
		{
			if (passiveSkill != null && passiveSkill.PassiveSkillData != null)
			{
				var textUI = Instantiate(skillTextPrefab, currentMonsterSkillContainer);
				textUI.text = passiveSkill.PassiveSkillData.skillName;
			}
		}
	}

	private void ClearSkillContainer()
	{
		if (currentMonsterSkillContainer == null) return;
		foreach (Transform child in currentMonsterSkillContainer)
		{
			Destroy(child.gameObject);
		}
	}
}
