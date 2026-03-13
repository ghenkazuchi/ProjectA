using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterProgressionSystem : RPGProgressionSystem
{
    private MonsterCharacter monster;
    private System.Random _random = new System.Random();

    public MonsterProgressionSystem(MonsterCharacter owner) : base(owner)
    {
        this.monster = owner;
    }

    public override void AddExp(int amount)
    {
        // Monsters don't typically gain EXP during combat in the same way, but can be implemented if needed.
    }

    public override void CheckForLevelUp()
    {
        // Stubbed for monsters.
    }

    public override int GetExpNeededForNextLevel()
    {
        return 0; // Stubbed for monsters.
    }

    public override void DistributeTraitPoints(int pointsToDistribute)
    {
        MonsterData monsterData = monster.entityData as MonsterData;
        if (monsterData == null) return;

        string gainedTraits = "";
        Trait dominant = monsterData.DominantTrait;
        float preferenceWeight = monsterData.DominantTraitPreferenceWeight;

        for (int p = 0; p < pointsToDistribute; p++)
        {
            if (monster.TraitListCache.Count == 0) break;

            List<float> weights = new List<float>();
            foreach (Trait t in monster.TraitListCache)
            {
                weights.Add(t == dominant ? preferenceWeight : 1.0f);
            }

            Trait chosenTrait = GetWeightedRandomTrait(new List<Trait>(monster.TraitListCache), weights);
            monster.IncrementTrait(chosenTrait, 1);
            gainedTraits += $"{chosenTrait}+1 ";
        }

        if (!string.IsNullOrEmpty(gainedTraits))
        {
            Debug.Log($"Monster {monsterData.EntityName} (Level {monster.Level}) gained Trait points: {gainedTraits}");
        }
    }

    private Trait GetWeightedRandomTrait(List<Trait> traits, List<float> weights)
    {
        float totalWeight = weights.Sum();
        float randomNumber = (float)_random.NextDouble() * totalWeight;

        for (int i = 0; i < traits.Count; i++)
        {
            if (randomNumber < weights[i])
            {
                return traits[i];
            }
            randomNumber -= weights[i];
        }
        return traits[traits.Count - 1];
    }

    public override void SetLevel(int targetLevel)
    {
        int oldLevel = (monster.Level > 0 ? monster.Level : 1);
        monster.SetLevelInternal(Mathf.Max(1, targetLevel));

        if (monster.Level > oldLevel)
        {
            int pointsForNewLevels = (monster.Level - oldLevel) * monster.BonusTraitPointPerLevel;
            DistributeTraitPoints(pointsForNewLevels);
        }
        monster.AddExclusiveSkill();
        monster.CheckAndLearnSkill(monster.Level);
        monster.CalculateAllStats();
    }
}
