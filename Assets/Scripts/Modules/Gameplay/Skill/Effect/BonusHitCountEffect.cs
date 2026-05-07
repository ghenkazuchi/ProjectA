using System.Collections;
using UnityEngine;

public class BonusHitCountEffect : EffectBase, IBonusHitCountModifier
{
    public int BonusHits { get; private set; }
    private bool isConsumed = false;

    public BonusHitCountEffect(BonusHitCountEffectData data, EntityBase owner, EntityBase target, int duration) 
        : base(data, owner, target, duration)
    {
        BonusHits = data.bonusHits;
    }

    public int GetBonusHitCount()
    {
        if (isConsumed) return 0;
        return BonusHits;
    }

	public override IEnumerator ApplyEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} next attack got extra hit.");
	}

	public IEnumerator Consume()
    {
        if (!isConsumed)
        {
            isConsumed = true;
            CurrentDuration = 0;
            yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} used their bonus hit count!");
        }
    }
}
