using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipEffectBinding
{
	public EquipEffectTrigger trigger;
	public EffectData effect;
	public float procChance = 1f;	
}
