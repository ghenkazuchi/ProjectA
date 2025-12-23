using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBanEffect : EffectBase, ITurnStart
{
	private readonly List<SkillDefinition> skillDefinitions = new List<SkillDefinition> { SkillDefinition.Spell };
	public SpellBanEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		TriggerPhase = EffectTriggerPhase.StartOfTurn;
	}

	public override IEnumerator ApplyEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} got there spell banned!");	
	}

	public IEnumerator OnTurnStart()
	{
		//Target.TurnControl.MergeBans(ActionBan.SpellSkill);
		//Target.TurnControl.MergeBannedSkillDefs(skillDefinitions);
		//Target.TurnControl.SourceEffectName = Name;
		//Target.TurnControl.SourceEffectRuntimeId = RuntimeId;
		var d = new TurnDirective { Priority = 50, SourceEffectRuntimeId = RuntimeId, SourceEffectName = Name };
		d.MergeBans(ActionBan.SpellSkill);
		d.MergeBannedSkillDefs(new[] { SkillDefinition.Spell });
		Target.ProposeTurnDirective(d);
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} can use spell again !");
		Target.TurnControl.BannedSkillDefs.Remove(SkillDefinition.Spell);
		Target.TurnControl.Bans &= ~ActionBan.SpellSkill;
	}
}
