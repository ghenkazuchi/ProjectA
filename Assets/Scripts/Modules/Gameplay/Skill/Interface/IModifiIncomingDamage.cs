using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModifiIncomingDamage 
{
	float GetInComingDamageModifier(EntityBase target, EntityBase source, int damage, BattleSystem battleSystem);
}
