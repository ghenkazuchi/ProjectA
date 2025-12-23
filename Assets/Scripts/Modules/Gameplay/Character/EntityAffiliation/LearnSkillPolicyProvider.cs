using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LearnSkillPolicyProvider 
{
	private static readonly ILearnSkillPolicy _partyPolicy = new PartyMemberLearnPolicy();
	private static readonly ILearnSkillPolicy _autoPolicy = new AutoLearnPolicy();

	public static ILearnSkillPolicy GetPolicy(EntityBase e)
	{
		return e.entityAffiliation switch
		{
			Affiliation.PartyMember => _partyPolicy,
			Affiliation.Recruitable => _autoPolicy,
			Affiliation.Enemy => _autoPolicy,
			_ => _autoPolicy
		};
	}
}
