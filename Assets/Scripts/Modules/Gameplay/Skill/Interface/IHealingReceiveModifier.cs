using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealingReceiveModifier 
{
	public IEnumerator OnHealing(EntityBase healer, EntityBase target, ref float healingAmount);
}
