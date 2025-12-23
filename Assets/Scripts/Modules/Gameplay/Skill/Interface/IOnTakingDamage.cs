using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnTakingDamage
{
	public IEnumerator OnTakingDamage(DamageContext context);
}
