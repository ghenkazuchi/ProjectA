using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSpiritInteractableObject : Interacable, IHeroSpiritInteractable
{
	[SerializeField] private List<BaseSkillData> listSkillData;
	[SerializeField] private BaseSkillData assignedSkill;

	private void Start()
	{
		OnCreation();
	}
	public void OnCreation()
	{
		int randomIndex = Random.Range(0, listSkillData.Count);
		assignedSkill = listSkillData[randomIndex];
	}

	public void OnLearned()
	{
		throw new System.NotImplementedException();
	}

	public override void TriggerInteraction()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnHeroSpiritInteractionPopupOpen, new object[] { assignedSkill, this }));
	}
}
