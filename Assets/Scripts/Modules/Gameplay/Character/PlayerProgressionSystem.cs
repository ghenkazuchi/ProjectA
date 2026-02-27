using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressionSystem : RPGProgressionSystem
{
    private PlayerCharacter player;
    private System.Random _random = new System.Random();

    public PlayerProgressionSystem(PlayerCharacter owner) : base(owner)
    {
        this.player = owner;
    }

    public override void AddExp(int amount)
    {
        player.currentExp += amount;
        Debug.Log($"{player.entityData.name} gained {amount} exp !");
        CheckForLevelUp();
    }

    public override void CheckForLevelUp()
    {
        int expNeeded = GetExpNeededForNextLevel();
        while (player.currentExp >= expNeeded && player.Level < GetMaxLevel())
        {
            player.level++;
            player.currentExp -= expNeeded;
            Debug.Log($"{player.entityData.name} reached Level {player.Level}!");
            DistributeTraitPoints(player.BonusTraitPointPerLevel);
            player.CalculateAllStats();
            player.CheckAndLearnSkill(player.Level);
            expNeeded = GetExpNeededForNextLevel();
            if (expNeeded <= 0 || expNeeded == int.MaxValue) break;
        }
    }

    public override int GetExpNeededForNextLevel()
    {
        int baseExpLevel1 = 24;
        float exponent = 1.5f;
        int currentLevelExp = Mathf.Max(1, player.Level);
        return Mathf.CeilToInt(baseExpLevel1 * Mathf.Pow(currentLevelExp, exponent) * player.GrowthModifier);
    }

    private int GetMaxLevel() { return 30; }

    public override void DistributeTraitPoints(int pointsToDistribute)
    {
        string gainedTraits = "";
        List<Trait> cache = new List<Trait>(player.TraitListCache);
        int n = cache.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            Trait value = cache[k];
            cache[k] = cache[n];
            cache[n] = value;
        }

        for (int i = 0; i < Mathf.Min(pointsToDistribute, cache.Count); i++)
        {
            Trait traitToIncrease = cache[i];
            player.IncrementTrait(traitToIncrease, 1);
            gainedTraits += $"{traitToIncrease}+1 ";
        }
        Debug.Log($"Player {player.entityData.name} (Level {player.Level}) gained Trait points: {gainedTraits}");
    }

    public override void SetLevel(int targetLevel)
    {
        int oldLevel = 1;
        player.level = Mathf.Max(1, targetLevel);
        if (player.level > oldLevel)
        {
            int pointsForNewLevels = (player.level - oldLevel) * player.BonusTraitPointPerLevel;
            DistributeTraitPoints(pointsForNewLevels);
        }
        player.CalculateAllStats();
        player.AddExclusiveSkill();
        player.CheckAndLearnSkill(player.Level);
    }
}
