using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLearnPolicy : ILearnSkillPolicy
{
	public void Resolve(LearnRequest request)
	{
		var entity = request.pc;
		if (entity.usableSkills.Count < entity.MaxActiveSkillSlots)
		{
			entity.usableSkills.Add(new ActiveSkill(request.skillData));
			entity.MarkActiveSkillLearned(request.skillData);
			request.onResolved?.Invoke(true);
			return;
		}
		int idx = UnityEngine.Random.Range(0, entity.usableSkills.Count);
		var toForget = entity.usableSkills[idx];
		entity.ForgetSkill(toForget);
		entity.usableSkills.RemoveAt(idx);
		entity.usableSkills.Add(new ActiveSkill(request.skillData));
		(entity as PlayerCharacter)?.MarkActiveSkillLearned(request.skillData);
		request.onResolved?.Invoke(true);
	}
}
