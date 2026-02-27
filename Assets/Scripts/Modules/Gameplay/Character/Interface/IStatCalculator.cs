using System.Collections.Generic;
using UnityEngine;

public interface IStatCalculator 
{
    float CalculateSingleStat(Stat statToCalculate, Dictionary<Trait, int> effTraits, EntityBase entity);
}
