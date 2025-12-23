using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeforeDealingDamage
{
	IEnumerator OnBeforeDealingDamage(DamageContext ctx);
}
