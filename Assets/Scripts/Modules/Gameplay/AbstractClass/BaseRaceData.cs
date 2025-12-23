using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRaceData : ScriptableObject
{
	public SerializableDictionaryBase<Trait, int> traitBonuses = new SerializableDictionaryBase<Trait, int> { };
}
