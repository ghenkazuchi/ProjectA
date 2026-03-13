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
        if (player.GetClassData == null || player.GetClassData.traitGrowthRates == null)
        {
            Debug.LogWarning("ClassData or traitGrowthRates is missing. Cannot distribute trait points.");
            return;
        }

        int levelsGained = Mathf.Max(1, pointsToDistribute / Mathf.Max(1, player.BonusTraitPointPerLevel));
        string gainedTraitsLog = "";

        for (int l = 0; l < levelsGained; l++)
        {
            foreach (Trait trait in player.TraitListCache)
            {
                int growthRate = 0;
                
                if (player.GetClassData != null && player.GetClassData.traitGrowthRates != null && player.GetClassData.traitGrowthRates.ContainsKey(trait))
                {
                    growthRate += player.GetClassData.traitGrowthRates[trait];
                }
                
                if (player.entityData != null && player.entityData.PersonalGrowthRates != null && player.entityData.PersonalGrowthRates.ContainsKey(trait))
                {
                    growthRate += player.entityData.PersonalGrowthRates[trait];
                }

                if (growthRate <= 0) continue;
                
                int totalGain = 0;
                while (growthRate >= 100)
                {
                    totalGain++;
                    growthRate -= 100;
                }
                
                if (growthRate > 0)
                {
                    int roll = _random.Next(1, 101); // 1 to 100
                    if (roll <= growthRate)
                    {
                        totalGain++;
                    }
                }
                
                if (totalGain > 0)
                {
                    player.IncrementTrait(trait, totalGain);
                    gainedTraitsLog += $"{trait}+{totalGain} ";
                }
            }
        }

        if (!string.IsNullOrEmpty(gainedTraitsLog))
        {
            Debug.Log($"Player {player.entityData.name} (Level {player.Level}) gained Trait points: {gainedTraitsLog}");
        }
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
        player.AddExclusiveSkill();
        player.CheckAndLearnSkill(player.Level);
        player.CalculateAllStats();
    }
}
