using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnAllyDealingDamage
{
	IEnumerator OnAllyDealingDamage(EntityBase ally, EntityBase target, DamageContext ctx);
}
