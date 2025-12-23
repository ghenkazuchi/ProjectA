using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModify
{
	public float ModifyStat(Stat statType, float currentValue,EntityBase target);
}
