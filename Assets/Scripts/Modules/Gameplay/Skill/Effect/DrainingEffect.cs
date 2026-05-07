using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainingEffect : EffectBase, IOnDealingDamage
{
    private DrainingEffectData drainData;

    public DrainingEffect(DrainingEffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
    {
        drainData = data;
    }

    public override IEnumerator ApplyEffect()
    {
        yield break;
    }

    public IEnumerator OnDealingDamage(DamageContext ctx)
    {
        // Only trigger if the owner of this effect is the one dealing damage
        if (ctx.Source != Owner) yield break;

        int drainAmount = Mathf.CeilToInt(ctx.EffectiveDamage * drainData.DrainingPercentage);
        if (drainAmount <= 0) yield break;

        // Play Heal animation and VFX
        var healingVFX = BattleSystem.Instance.vfxLib.GetHealingVFX();
        var targetUnit = BattleSystem.Instance.FindBattleUnitForEntityPublic(Owner);
        var animator = targetUnit?.GetAnimator();
        if (animator != null)
        {
            yield return animator.PlayHealingAnimation(healingVFX);
        }

        yield return BattleSystem.Instance.ShowDialog($"{Owner.entityData.EntityName} drains {ctx.Target.entityData.EntityName}'s life force!");

        var healingContext = new HealingContext();
        healingContext.Reset(Owner, Owner, drainAmount);
        yield return BattleSystem.Instance.HandleEntityGotHeal(healingContext);
    }

    public override IEnumerator RemoveEffect()
    {
        yield break;
    }
}
