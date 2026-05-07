using System.Collections;
using UnityEngine;

public class StealthEffect : EffectBase, IOnTakingDamage
{
    private bool isBroken = false;

    public StealthEffect(StealthEffectData data, EntityBase owner, EntityBase target, int duration) 
        : base(data, owner, target, duration)
    {
    }

    public override IEnumerator ApplyEffect()
    {
        Target.stealthCount++;
        // Play a specific stealth VFX if you have one, or just a generic buff animation
        yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} enters stealth!");
    }

    public override IEnumerator RemoveEffect()
    {
        if (!isBroken)
        {
            Target.stealthCount--;
        }
        else
        {
            // If it was broken, the count was already decremented in OnTakingDamage
        }
        yield break;
    }

    public IEnumerator OnTakingDamage(DamageContext ctx)
    {
        if (isBroken) yield break;

        // If the owner takes damage (e.g. from an AoE or DOT), stealth drops!
        if (ctx.EffectiveDamage > 0)
        {
            isBroken = true;
            Target.stealthCount = Mathf.Max(0, Target.stealthCount - 1);
            
            yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName}'s stealth was broken!");
            
            // Immediately mark for removal so the duration tracker cleans it up
            CurrentDuration = 0; 
        }
    }
}
