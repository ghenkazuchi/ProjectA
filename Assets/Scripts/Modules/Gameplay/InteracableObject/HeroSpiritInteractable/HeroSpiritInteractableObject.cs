using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSpiritInteractableObject : Interacable, IHeroSpiritInteractable
{
	[SerializeField] private List<BaseSkillData> listSkillData;
	[SerializeField] private UnlockableSkillPool unlockableSkillPool;
	[SerializeField] private BaseSkillData assignedSkill;

	private void Start()
	{
		OnCreation();
	}
	public void OnCreation()
	{
		List<BaseSkillData> availableSkills = GetResolvedSkillPool();
		if (availableSkills.Count == 0)
		{
			assignedSkill = null;
			return;
		}

		int randomIndex = Random.Range(0, availableSkills.Count);
		assignedSkill = availableSkills[randomIndex];
	}

	public void OnLearned()
	{
		Destroy(gameObject);
	}

	public override void TriggerInteraction()
	{
		if (assignedSkill == null)
		{
			OnCreation();
		}
		if (assignedSkill == null)
		{
			Debug.LogWarning("HeroSpiritInteractableObject: No skill available to learn.");
			return;
		}

		MessageManager.Instance.SendMessage(new Message(MessageType.OnHeroSpiritInteractionPopupOpen, new object[] { assignedSkill, this }));
	}

	private List<BaseSkillData> GetResolvedSkillPool()
	{
		if (unlockableSkillPool == null || DataManager.Instance?.Achievements == null)
		{
			return new List<BaseSkillData>(listSkillData);
		}

		return DataManager.Instance.Achievements.GetSkillsForPool(unlockableSkillPool, listSkillData);
	}
}
