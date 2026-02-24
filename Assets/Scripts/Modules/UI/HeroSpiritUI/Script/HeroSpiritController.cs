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
	private void Awake()
	{
		Hide();
		//learnButton.onClick.AddListener => OnLearnButtonClicked();
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
	}
	public void Reset()
	{
		asignedSkillData = null;
		asignedSkillTypeIcon.sprite = null;
		asignedSkillNameText.text = "";
		asignedSkillDescriptionText.text = "";
	}
	public void InitPopup(BaseSkillData skillData)
	{
		if (skillData == null)
		{
			Debug.LogError("HeroSpiritController: InitPopup - skillData is null");
			return;
		}
		asignedSkillData = skillData;
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

	}
	private void OnCloseButtonClicked()
	{
		Hide();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnHeroSpiritInteractionPopupClose));
	}
}
