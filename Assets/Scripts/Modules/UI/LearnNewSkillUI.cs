using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearnNewSkillUI : MonoBehaviour
{
	[SerializeField] private SkillButtonUI[] skillButton;
	[SerializeField] private Button cancelButton;
	[SerializeField] private TextMeshProUGUI titleText;

	private EntityBase currentCharacter;
	private ActiveSkill skillWantToLearn;
	private Action<int> onChooseIndex; 
	private Action onCancel;
	public void Show(EntityBase character, ActiveSkill skillToLearn, Action<int> onChooseIndex, Action onCancel)
	{
		this.currentCharacter = character;
		this.skillWantToLearn = skillToLearn;
		this.onChooseIndex = onChooseIndex;
		this.onCancel = onCancel;

		titleText.text = $"{currentCharacter.entityData.EntityName} wants to learn {skillWantToLearn.SkillData.skillName}";

		SetupButtons();

		cancelButton.onClick.RemoveAllListeners();
		cancelButton.onClick.AddListener(() =>
		{
			this.onCancel?.Invoke();
		});
	}

	private void SetupButtons()
	{
		var skills = currentCharacter.GetUsableSkills();
		for (int i = 0; i < skillButton.Length; i++)
		{
			if (i < skills.Count)
			{
				skillButton[i].gameObject.SetActive(true);
				skillButton[i].Setup(skills[i]);
				int captured = i;
				skillButton[i].SetOnClick(() => onChooseIndex?.Invoke(captured));
			}
			else
			{
				skillButton[i].gameObject.SetActive(false);
			}
		}
	}
}
