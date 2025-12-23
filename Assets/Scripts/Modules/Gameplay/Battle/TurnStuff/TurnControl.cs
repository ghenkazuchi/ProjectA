using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ActionBan : int
{
	None = 0,
	Defense = 1 << 0,
	BasicAttack = 1 << 1,
	SpellSkill = 1 << 2,
	BattleArtSkill = 1 << 3,	
}

public enum ForcedActionKind
{
	None,
	BasicAttack,
	Skill,
}
public class TurnDirective
{
	public bool SkipThisTurn;
	public string SkipReason = string.Empty;

	public ActionBan Bans = ActionBan.None;
	public ForcedActionKind ForcedAction = ForcedActionKind.None;
	public EntityBase ForcedTarget;
	public int ForcedSkillIndex = -1;

	public int Priority;

	public int SourceEffectRuntimeId;
	public string SourceEffectName;

	public void Clear()
	{
		SkipThisTurn = false;
		SkipReason = string.Empty;
		Bans = ActionBan.None;
		ForcedAction = ForcedActionKind.None;
		ForcedTarget = null;
		ForcedSkillIndex = -1;
		Priority = 0;
		SourceEffectRuntimeId = 0;
		SourceEffectName = null;
	}
	public void Propose(TurnDirective proposal)
	{
		if (proposal == null) return;
		if (proposal.Priority < this.Priority)
		{
			this.MergeBans(proposal.Bans);
			this.MergeBannedSkillDefs(proposal.BannedSkillDefs);
			return;
		}
		SkipThisTurn = proposal.SkipThisTurn;
		SkipReason = proposal.SkipReason ?? string.Empty;
		Bans = proposal.Bans;
		ForcedAction = proposal.ForcedAction;
		ForcedTarget = proposal.ForcedTarget;
		ForcedSkillIndex = proposal.ForcedSkillIndex;
		Priority = proposal.Priority;
		SourceEffectRuntimeId = proposal.SourceEffectRuntimeId;
		SourceEffectName = proposal.SourceEffectName;

		this.MergeBans(proposal.Bans);
		this.MergeBannedSkillDefs(proposal.BannedSkillDefs);
	}
	public void MergeBans(ActionBan bans) => Bans |= bans;

	public HashSet<SkillDefinition> BannedSkillDefs = new HashSet<SkillDefinition>();
	public void MergeBannedSkillDefs(IEnumerable<SkillDefinition> defs)
	{
		if (defs == null) return;
		foreach (var def in defs)
			BannedSkillDefs.Add(def);
	}
}