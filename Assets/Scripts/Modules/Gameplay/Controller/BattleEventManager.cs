using System.Collections;
using System.Collections.Generic;

public static class BattleEventManager
{
	//Event bus for battle-related events
	public static IEnumerator TriggerBeforeDealingDamage(EntityBase attacker, EntityBase target, DamageContext ctx)
    {
        yield return attacker.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBeforeDealingDamage, target, ctx);
        var allEffect = attacker.GetAllEffect();
        if (allEffect != null)
        {
            for (int i = 0; i < allEffect.Count; i++)
            {
                var eff = allEffect[i];
                if (eff is IBeforeDealingDamage hook)
                    yield return hook.OnBeforeDealingDamage(ctx);
            }
        }
    }

    public static IEnumerator TriggerOnTakingDamage(EntityBase defender, DamageContext ctx)
    {
        yield return defender.PassiveSkillRunner.Trigger(PassiveTrigger.OnTakingDamage, defender, ctx);
        var allEffect = defender.GetAllEffect();
        if (allEffect != null)
        {
            for (int i = 0; i < allEffect.Count; i++)
            {
                var eff = allEffect[i];
                if (eff is IOnTakingDamage hook)
                {
                    yield return hook.OnTakingDamage(ctx);
                }
            }
        }

        yield return defender.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnTakingDamage, defender, ctx);
    }

    public static IEnumerator TriggerDealingDamage(EntityBase attacker, EntityBase target, DamageContext ctx)
    {
        yield return attacker.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnDealingDamage, target, ctx);
        yield return attacker.PassiveSkillRunner.Trigger(PassiveTrigger.OnDealingDamage, attacker, ctx);
        var allEffect = attacker.GetAllEffect();
        if (allEffect != null)
        {
            for (int i = 0; i < allEffect.Count; i++)
            {
                var eff = allEffect[i];
                if (eff is IOnDealingDamage hook)
                {
                    yield return hook.OnDealingDamage(ctx);
                }
            }
        }

        List<EntityBase> allies = (attacker is PlayerCharacter) 
            ? BattleSystem.Instance.playerParty.GetAllEntitiesInParty() 
            : BattleSystem.Instance.monsterParty.GetAllEntitiesInParty();
            
        foreach (var ally in allies)
        {
            if (ally == attacker || ally.GetCurrentHP() <= 0) continue;
            var allyEffects = ally.GetAllEffect();
            if (allyEffects != null)
            {
                for (int i = 0; i < allyEffects.Count; i++)
                {
                    if (allyEffects[i] is IOnAllyDealingDamage allyHook)
                    {
                        yield return allyHook.OnAllyDealingDamage(attacker, target, ctx);
                    }
                }
            }
        }
    }

    public static IEnumerator TriggerBeforeTakingDamage(EntityBase defender, DamageContext ctx)
    {
        yield return defender.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBeforeTakingDamage, defender, ctx);
        yield return defender.PassiveSkillRunner.Trigger(PassiveTrigger.OnBeforeTakingDamage, defender, ctx);
        var allEffect = defender.GetAllEffect();
        if (allEffect != null)
        {
            for (int i = 0; i < allEffect.Count; i++)
            {
                var eff = allEffect[i];
                if (eff is IBeforeTakingDamage hook)
                    yield return hook.OnBeforeTakingDamage(ctx);
            }
        }
    }

    public static IEnumerator TriggerOnHealingReceived(EntityBase target, HealingContext ctx)
    {
        var effects = target?.GetAllEffect();
        if (effects == null) yield break;
        for (int i = 0; i < effects.Count; i++)
        {
            var e = effects[i];
            if (e is IOnHealingReceived hook)
            {
                yield return hook.OnHealingReceived(ctx);
            }
        }
    }

    public static void ResetEffectOnEntities(IEnumerable<EntityBase> allEntities)
    {
        if (allEntities == null) return;
        UnityEngine.Debug.Log("Resetting Effect");
        foreach (var entity in allEntities)
        {
            entity?.ResetEffectAfterBattle();
            entity?.PassiveSkillRunner?.ResetPassiveEffect();
        }
    }
}
