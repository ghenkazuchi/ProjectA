using System.Collections.Generic;
using UnityEngine;

public abstract class RPGProgressionSystem
{
    protected EntityBase entity;

    public RPGProgressionSystem(EntityBase owner)
    {
        this.entity = owner;
    }

    public abstract void AddExp(int amount);
    public abstract void CheckForLevelUp();
    public abstract int GetExpNeededForNextLevel();
    public abstract void DistributeTraitPoints(int pointsToDistribute);
    public abstract void SetLevel(int targetLevel);
}
