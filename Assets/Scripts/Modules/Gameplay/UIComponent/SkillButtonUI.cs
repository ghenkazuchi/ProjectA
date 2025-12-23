using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
	[SerializeField] private ActiveSkill skill;
	[SerializeField] private TextMeshProUGUI skillNameText;
	[SerializeField] private Button button; // thêm

	public void Setup(ActiveSkill skill)
	{
		this.skill = skill;
		if (skill == null)
		{
			skillNameText.text = "---";
			if (button) button.interactable = false;
		}
		else
		{
			skillNameText.text = skill.SkillData.skillName;
			if (button) button.interactable = true;
		}
	}

	public void SetOnClick(Action onClick)
	{
		if (!button) return;
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => onClick?.Invoke());
	}
}
