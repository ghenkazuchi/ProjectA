using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BattleArtBanEffect : EffectBase, ITurnStart
{
	public BattleArtBanEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} got there battle art banned!");
	}
	public IEnumerator OnTurnStart()
	{
		var d = new TurnDirective { Priority = 50, SourceEffectRuntimeId = RuntimeId, SourceEffectName = Name };
		d.MergeBans(ActionBan.BattleArtSkill);
		d.MergeBannedSkillDefs(new[] { SkillDefinition.BattleArt });
		Target.ProposeTurnDirective(d);
		yield break;
	}
	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} can use battle art again!");
		Target.TurnControl.BannedSkillDefs.Remove(SkillDefinition.BattleArt);
		Target.TurnControl.Bans &= ~ActionBan.BattleArtSkill;
	}
}
