using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillLearnManager : Singleton<SkillLearnManager>
{
	[SerializeField] private LearnNewSkillUI learnUI;
	private readonly Queue<LearnRequest> queue = new();
	private bool isShowing = false;

	public void Enqueue(PlayerCharacter pc, ActiveSkillData skillToLearn, Action<bool> onResolved)
	{
		queue.Enqueue(new LearnRequest { pc = pc, skillData = skillToLearn, onResolved = onResolved });
		TryDequeueAndShow();
	}
	private void TryDequeueAndShow()
	{
		if (isShowing) return;
		if (queue.Count == 0) return;

		var req = queue.Dequeue();
		isShowing = true;

		learnUI.gameObject.SetActive(true);
		learnUI.Show(
			character: req.pc,
			skillToLearn: new ActiveSkill(req.skillData),
			onChooseIndex: (idx) =>
			{
				bool learned = false;
				if (idx >= 0 && idx < req.pc.usableSkills.Count)
				{
					var toForget = req.pc.usableSkills[idx];
					req.pc.ForgetSkill(toForget);
					req.pc.usableSkills.Add(new ActiveSkill(req.skillData));
					learned = true;
					req.pc.MarkActiveSkillLearned(req.skillData);
				}
				req.onResolved?.Invoke(learned);
				CloseAndContinue();
			},
			onCancel: () =>
			{
				req.onResolved?.Invoke(false);
				CloseAndContinue();
			}
		);
	}

	private void CloseAndContinue()
	{
		learnUI.gameObject.SetActive(false);
		isShowing = false;
		TryDequeueAndShow();
	}
}
