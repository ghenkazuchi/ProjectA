using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveEffectData : ScriptableObject
{
	public abstract PassiveEffectBase CreateRuntimeEffect();
}
