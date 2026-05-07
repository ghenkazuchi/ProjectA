using System.Collections;
using UnityEngine;

public class TauntEffect : EffectBase
{
    public TauntEffect(TauntEffectData data, EntityBase owner, EntityBase target, int duration) 
        : base(data, owner, target, duration)
    {
    }

    public override IEnumerator ApplyEffect()
    {
        Target.TauntedBy = Owner;
        yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} is taunted by {Owner.entityData.EntityName}!");
    }

    public override IEnumerator RemoveEffect()
    {
        if (Target.TauntedBy == Owner)
        {
            Target.TauntedBy = null;
        }
        yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} is no longer taunted.");
    }
}
