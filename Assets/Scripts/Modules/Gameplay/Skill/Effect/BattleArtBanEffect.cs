using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BattleArtBanEffect : EffectBase, ITurnStart
{
	public BattleArtBanEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} got there battle art and basic attack banned!");
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
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} can use battle art and basic attack again !");
		Target.TurnControl.BannedSkillDefs.Remove(SkillDefinition.BattleArt);
		Target.TurnControl.Bans &= ~ActionBan.BattleArtSkill;
	}
}
