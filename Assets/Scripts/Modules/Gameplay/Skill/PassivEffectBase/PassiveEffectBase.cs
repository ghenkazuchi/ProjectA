using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveEffectBase 
{
	public abstract IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem,params object[] args);
}
