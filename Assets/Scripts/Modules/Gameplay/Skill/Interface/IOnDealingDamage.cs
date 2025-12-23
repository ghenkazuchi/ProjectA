using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnDealingDamage
{
	public IEnumerator OnDealingDamage(DamageContext ctx);
}
