using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMemberLearnPolicy : ILearnSkillPolicy
{
	public void Resolve(LearnRequest request)
	{
		var playerCharacter = (PlayerCharacter)request.pc;

		if (playerCharacter.usableSkills.Count < playerCharacter.MaxActiveSkillSlots)
		{
			playerCharacter.usableSkills.Add(new ActiveSkill(request.skillData));
			playerCharacter.MarkActiveSkillLearned(request.skillData);
			request.onResolved?.Invoke(true);
			return;
		}
		SkillLearnManager.Instance.Enqueue(
			pc: playerCharacter,
			skillToLearn: request.skillData,
			onResolved: (learned) =>
			{
				if (learned)
				{
					playerCharacter.MarkActiveSkillLearned(request.skillData);
				}
				request.onResolved?.Invoke(learned);
			}
		);

	}
}
