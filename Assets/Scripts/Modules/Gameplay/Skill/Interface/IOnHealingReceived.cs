using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnHealingReceived 
{
	public IEnumerator OnHealingReceived( HealingContext ctx);
}
