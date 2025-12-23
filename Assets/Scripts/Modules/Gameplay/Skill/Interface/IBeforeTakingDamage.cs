using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeforeTakingDamage 
{
	IEnumerator OnBeforeTakingDamage(DamageContext ctx);
}
