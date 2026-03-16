using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSpiritController : MonoBehaviour
{
	[SerializeField] private BaseSkillData asignedSkillData;
	[SerializeField] private Image asignedSkillTypeIcon;
	[SerializeField] private TextMeshProUGUI asignedSkillNameText;
	[SerializeField] private TextMeshProUGUI asignedSkillDescriptionText;
	[SerializeField] private Button learnButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private CanvasGroup canvasGroup;

	[Header("Skill Type Icon settings")]

	[SerializeField] private Sprite battleArtIcon;
	[SerializeField] private Sprite spellIcon;
	[SerializeField] private Sprite almightyIcon;
	[SerializeField] private Sprite passiveIcon;
	private IHeroSpiritInteractable currentInteractable;
	private bool selectingPartyMember;
	private bool selectingReplacementSkill;
	private PlayerCharacter selectedPartyMember;
	private string statusMessage;
	private Vector2 partyScroll;
	private Vector2 replacementScroll;
	private void Awake()
	{
		Hide();
		if (learnButton != null)
		{
			learnButton.onClick.AddListener(OnLearnButtonClicked);
		}
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(OnCloseButtonClicked);
		}
	}
	public void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}
	public void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		selectingPartyMember = false;
		selectingReplacementSkill = false;
		selectedPartyMember = null;
	}
	public void Reset()
	{
		asignedSkillData = null;
		currentInteractable = null;
		asignedSkillTypeIcon.sprite = null;
		asignedSkillNameText.text = "";
		asignedSkillDescriptionText.text = "";
		statusMessage = "";
		selectingPartyMember = false;
		selectingReplacementSkill = false;
		selectedPartyMember = null;
	}
	public void InitPopup(BaseSkillData skillData, IHeroSpiritInteractable interactable)
	{
		Reset();
		if (skillData == null)
		{
			Debug.LogError("HeroSpiritController: InitPopup - skillData is null");
			return;
		}
		asignedSkillData = skillData;
		currentInteractable = interactable;
		asignedSkillNameText.text = asignedSkillData.skillName;
		asignedSkillDescriptionText.text = asignedSkillData.skillDescription;
		if (asignedSkillData.skillType == SkillType.Passive)
		{
			asignedSkillTypeIcon.sprite = passiveIcon;
		}
		else
		{
			ActiveSkillData activeSkillData = asignedSkillData as ActiveSkillData;
			if (activeSkillData == null)
			{
				Debug.LogError("HeroSpiritController: InitPopup - activeSkillData is null");
				return;
			}
			switch (activeSkillData.skillDefinition)
			{
				case SkillDefinition.BattleArt:
					asignedSkillTypeIcon.sprite = battleArtIcon;
					break;
				case SkillDefinition.Spell:
					asignedSkillTypeIcon.sprite = spellIcon;
					break;
				case SkillDefinition.Almighty:
					asignedSkillTypeIcon.sprite = almightyIcon;
					break;
				default:
					Debug.LogError("HeroSpiritController: InitPopup - Unknown activeSkillCategory");
					break;
			}
		}
	}
	private void OnLearnButtonClicked()
	{
		if (asignedSkillData == null)
		{
			statusMessage = "No skill is available.";
			return;
		}

		statusMessage = "Choose a party member.";
		selectingPartyMember = true;
		selectingReplacementSkill = false;
		selectedPartyMember = null;
	}
	private void OnCloseButtonClicked()
	{
		Reset();
		Hide();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnHeroSpiritInteractionPopupClose));
	}

	private void OnGUI()
	{
		if (canvasGroup == null || canvasGroup.alpha <= 0f)
		{
			return;
		}

		if (!selectingPartyMember && !selectingReplacementSkill)
		{
			return;
		}

		float width = 520f;
		float height = 340f;
		Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
		GUI.Box(rect, selectingReplacementSkill ? "Choose Skill To Replace" : "Choose Party Member");

		GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 30f, rect.width - 24f, rect.height - 42f));
		if (!string.IsNullOrEmpty(statusMessage))
		{
			GUILayout.Label(statusMessage);
			GUILayout.Space(8f);
		}

		if (selectingPartyMember)
		{
			DrawPartySelection();
		}
		else if (selectingReplacementSkill)
		{
			DrawReplacementSelection();
		}
		GUILayout.EndArea();
	}

	private void DrawPartySelection()
	{
		List<PlayerCharacter> partyMembers = PlayerParty.Instance != null
			? PlayerParty.Instance.GetAllPlayerCharacter()
			: new List<PlayerCharacter>();

		partyScroll = GUILayout.BeginScrollView(partyScroll);
		foreach (var member in partyMembers)
		{
			if (member == null) continue;
			string label = BuildPartyMemberLabel(member);
			if (GUILayout.Button(label, GUILayout.Height(40f)))
			{
				HandlePartyMemberSelection(member);
			}
		}
		GUILayout.EndScrollView();

		if (GUILayout.Button("Cancel"))
		{
			selectingPartyMember = false;
			statusMessage = "";
		}
	}

	private void DrawReplacementSelection()
	{
		if (selectedPartyMember == null || !(asignedSkillData is ActiveSkillData activeSkillData))
		{
			selectingReplacementSkill = false;
			selectingPartyMember = true;
			statusMessage = "Choose a party member.";
			return;
		}

		replacementScroll = GUILayout.BeginScrollView(replacementScroll);
		for (int i = 0; i < selectedPartyMember.usableSkills.Count; i++)
		{
			var knownSkill = selectedPartyMember.usableSkills[i];
			if (knownSkill == null) continue;
			if (GUILayout.Button($"Replace {knownSkill.SkillData.skillName}", GUILayout.Height(36f)))
			{
				selectedPartyMember.ForgetSkill(knownSkill);
				selectedPartyMember.usableSkills.Add(new ActiveSkill(activeSkillData));
				selectedPartyMember.MarkActiveSkillLearned(activeSkillData);
				CompleteLearning(selectedPartyMember);
				return;
			}
		}
		GUILayout.EndScrollView();

		if (GUILayout.Button("Back"))
		{
			selectingReplacementSkill = false;
			selectingPartyMember = true;
			selectedPartyMember = null;
			statusMessage = "Choose a party member.";
		}
	}

	private string BuildPartyMemberLabel(PlayerCharacter member)
	{
		if (member == null)
		{
			return "Unknown";
		}

		if (asignedSkillData is ActiveSkillData activeSkillData)
		{
			if (member.KnowsActiveSkill(activeSkillData))
			{
				return $"{member.entityData.EntityName} - already knows this skill";
			}

			if (member.usableSkills.Count >= member.MaxActiveSkillSlots)
			{
				return $"{member.entityData.EntityName} - choose replacement";
			}

			return $"{member.entityData.EntityName} - can learn";
		}

		if (asignedSkillData is PassiveSkillData passiveSkillData)
		{
			return member.KnowsPassiveSkill(passiveSkillData)
				? $"{member.entityData.EntityName} - already knows this passive"
				: $"{member.entityData.EntityName} - can learn";
		}

		return $"{member.entityData.EntityName} - unsupported skill type";
	}

	private void HandlePartyMemberSelection(PlayerCharacter member)
	{
		if (member == null || asignedSkillData == null)
		{
			statusMessage = "Invalid learning target.";
			return;
		}

		if (asignedSkillData is ActiveSkillData activeSkillData)
		{
			if (member.KnowsActiveSkill(activeSkillData))
			{
				statusMessage = $"{member.entityData.EntityName} already knows {activeSkillData.skillName}.";
				return;
			}

			if (member.usableSkills.Count < member.MaxActiveSkillSlots)
			{
				member.usableSkills.Add(new ActiveSkill(activeSkillData));
				member.MarkActiveSkillLearned(activeSkillData);
				CompleteLearning(member);
				return;
			}

			selectedPartyMember = member;
			selectingPartyMember = false;
			selectingReplacementSkill = true;
			statusMessage = $"Choose a skill to replace for {member.entityData.EntityName}.";
			return;
		}

		if (asignedSkillData is PassiveSkillData passiveSkillData)
		{
			if (!member.TryLearnPassiveSkill(passiveSkillData))
			{
				statusMessage = $"{member.entityData.EntityName} already knows {passiveSkillData.skillName}.";
				return;
			}

			CompleteLearning(member);
			return;
		}

		statusMessage = "This skill type is not supported.";
	}

	private void CompleteLearning(PlayerCharacter member)
	{
		statusMessage = $"{member.entityData.EntityName} learned {asignedSkillData.skillName}!";
		currentInteractable?.OnLearned();
		OnCloseButtonClicked();
	}
}
