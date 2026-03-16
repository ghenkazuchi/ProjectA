using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class AchievementService
{
	private sealed class AchievementProgressState
	{
		public bool completed;
		public int completionOrder;
		public List<int> conditionProgress = new List<int>();
	}

	private readonly AchievementDatabase database;
	private readonly IAchievementStorage storage;
	private readonly Dictionary<string, AchievementDefinition> definitionsById = new Dictionary<string, AchievementDefinition>();
	private readonly Dictionary<string, AchievementProgressState> statesById = new Dictionary<string, AchievementProgressState>();
	private readonly Dictionary<string, List<EquipableBaseData>> unlockedShopEquipablesByPool = new Dictionary<string, List<EquipableBaseData>>();
	private readonly Dictionary<string, List<EquipableBaseData>> unlockedChestEquipablesByPool = new Dictionary<string, List<EquipableBaseData>>();
	private readonly Dictionary<string, List<BaseSkillData>> unlockedSkillsByPool = new Dictionary<string, List<BaseSkillData>>();
	private int nextCompletionOrder = 1;

	public event Action<AchievementDefinition> OnAchievementCompleted;

	public AchievementService(AchievementDatabase database, IAchievementStorage storage)
	{
		this.database = database;
		this.storage = storage;
		InitializeDefinitions();
		Load();
		RebuildUnlockedRewards();
	}

	public bool HasDefinitions => database != null && database.Achievements != null && database.Achievements.Count > 0;

	public IReadOnlyList<AchievementDefinition> GetDefinitions()
	{
		if (database == null || database.Achievements == null)
		{
			return Array.Empty<AchievementDefinition>();
		}

		return database.Achievements;
	}

	public bool IsCompleted(AchievementDefinition definition)
	{
		if (definition == null) return false;
		return statesById.TryGetValue(definition.AchievementId, out AchievementProgressState state) && state.completed;
	}

	public int GetCompletionOrder(AchievementDefinition definition)
	{
		if (definition == null) return 0;
		return statesById.TryGetValue(definition.AchievementId, out AchievementProgressState state) ? state.completionOrder : 0;
	}

	public int GetConditionProgress(AchievementDefinition definition, int conditionIndex)
	{
		if (definition == null) return 0;
		AchievementProgressState state = EnsureState(definition);
		if (conditionIndex < 0 || conditionIndex >= state.conditionProgress.Count)
		{
			return 0;
		}

		return state.conditionProgress[conditionIndex];
	}

	public List<string> GetRewardSummaries(AchievementDefinition definition)
	{
		List<string> summaries = new List<string>();
		if (definition == null)
		{
			return summaries;
		}

		foreach (var reward in definition.Rewards)
		{
			if (reward == null) continue;
			summaries.Add(reward.GetSummary());
		}

		return summaries;
	}

	public List<EquipableBaseData> GetEquipablesForPool(UnlockableEquipablePool pool, IList<EquipableBaseData> fallbackBase)
	{
		List<EquipableBaseData> result = new List<EquipableBaseData>();
		AddDistinct(result, fallbackBase);
		if (pool == null)
		{
			return result;
		}

		AddDistinct(result, pool.BaseContents);
		if (pool.SourceKind == UnlockableEquipableSourceKind.Shop)
		{
			AddDistinct(result, GetUnlockedEquipables(unlockedShopEquipablesByPool, pool.PoolId));
		}
		else
		{
			AddDistinct(result, GetUnlockedEquipables(unlockedChestEquipablesByPool, pool.PoolId));
		}

		return result;
	}

	public List<BaseSkillData> GetSkillsForPool(UnlockableSkillPool pool, IList<BaseSkillData> fallbackBase)
	{
		List<BaseSkillData> result = new List<BaseSkillData>();
		AddDistinct(result, fallbackBase);
		if (pool == null)
		{
			return result;
		}

		AddDistinct(result, pool.BaseContents);
		if (unlockedSkillsByPool.TryGetValue(pool.PoolId, out List<BaseSkillData> unlocked))
		{
			AddDistinct(result, unlocked);
		}

		return result;
	}

	public void RecordMonsterKill(MonsterCharacter monster)
	{
		if (monster == null) return;
		RecordEvent(new AchievementEventData
		{
			Kind = AchievementEventKind.MonsterKill,
			Monster = monster
		});
	}

	public void RecordBattleWin()
	{
		RecordBattleWin(-1f, 0, 0, default, 0, null);
	}

	public void RecordBattleWin(float partyHealthRatio, int alivePartyMemberCount, int totalPartyMemberCount)
	{
		RecordBattleWin(partyHealthRatio, alivePartyMemberCount, totalPartyMemberCount, default, 0, null);
	}

	public void RecordBattleWin(
		float partyHealthRatio,
		int alivePartyMemberCount,
		int totalPartyMemberCount,
		BattleType battleType,
		int battleItemUseCount,
		IList<MonsterCharacter> battleMonsters)
	{
		RecordEvent(new AchievementEventData
		{
			Kind = AchievementEventKind.BattleWin,
			PartyHealthRatio = partyHealthRatio,
			AlivePartyMemberCount = Mathf.Max(0, alivePartyMemberCount),
			TotalPartyMemberCount = Mathf.Max(0, totalPartyMemberCount),
			BattleType = battleType,
			BattleItemUseCount = Mathf.Max(0, battleItemUseCount),
			BattleMonsters = battleMonsters != null ? new List<MonsterCharacter>(battleMonsters) : null
		});
	}

	public void RecordShopPurchase(EquipableBaseData equipable)
	{
		RecordEvent(new AchievementEventData
		{
			Kind = AchievementEventKind.ShopPurchase,
			Equipable = equipable
		});
	}

	public void RecordChestOpen()
	{
		RecordEvent(new AchievementEventData { Kind = AchievementEventKind.ChestOpen });
	}

	public void RecordRecruit()
	{
		RecordRecruit(null, null);
	}

	public void RecordRecruit(BaseEntityData characterData, RecruitableCharacterTemplate template)
	{
		RecordEvent(new AchievementEventData
		{
			Kind = AchievementEventKind.Recruit,
			RecruitedCharacterData = characterData,
			RecruitedTemplate = template
		});
	}

	public void RecordInteraction(Interacable interactable)
	{
		if (interactable == null)
		{
			return;
		}

		SpawnableObject spawnableObject = interactable.spawnableData;
		RecordEvent(new AchievementEventData
		{
			Kind = AchievementEventKind.Interaction,
			HasInteractableType = spawnableObject != null,
			InteractableType = spawnableObject != null ? spawnableObject.interacableType : default,
			SpawnableObject = spawnableObject,
			InteractionKey = spawnableObject != null ? spawnableObject.GetKey() : interactable.name
		});
	}

	private void InitializeDefinitions()
	{
		if (database == null || database.Achievements == null)
		{
			return;
		}

		foreach (var definition in database.Achievements)
		{
			if (definition == null) continue;
			string id = definition.AchievementId;
			if (string.IsNullOrWhiteSpace(id) || definitionsById.ContainsKey(id))
			{
				continue;
			}

			definitionsById[id] = definition;
			EnsureState(definition);
		}
	}

	private void Load()
	{
		AchievementProgressFileDTO dto = storage?.Load() ?? new AchievementProgressFileDTO();
		nextCompletionOrder = Mathf.Max(1, dto.nextCompletionOrder);

		foreach (var entry in dto.achievements)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.achievementId)) continue;
			if (!definitionsById.TryGetValue(entry.achievementId, out AchievementDefinition definition)) continue;

			AchievementProgressState state = EnsureState(definition);
			state.completed = entry.completed;
			state.completionOrder = entry.completionOrder;
			for (int i = 0; i < state.conditionProgress.Count && i < entry.conditionProgress.Count; i++)
			{
				state.conditionProgress[i] = Mathf.Max(0, entry.conditionProgress[i]);
			}

			if (state.completed && state.completionOrder >= nextCompletionOrder)
			{
				nextCompletionOrder = state.completionOrder + 1;
			}
		}
	}

	private void Save()
	{
		if (storage == null) return;

		AchievementProgressFileDTO dto = new AchievementProgressFileDTO
		{
			nextCompletionOrder = nextCompletionOrder
		};

		foreach (var definition in GetDefinitions())
		{
			if (definition == null) continue;
			AchievementProgressState state = EnsureState(definition);
			dto.achievements.Add(new AchievementProgressEntryDTO
			{
				achievementId = definition.AchievementId,
				completed = state.completed,
				completionOrder = state.completionOrder,
				conditionProgress = new List<int>(state.conditionProgress)
			});
		}

		storage.Save(dto);
	}

	private AchievementProgressState EnsureState(AchievementDefinition definition)
	{
		string id = definition.AchievementId;
		if (!statesById.TryGetValue(id, out AchievementProgressState state))
		{
			state = new AchievementProgressState();
			statesById[id] = state;
		}

		int targetCount = definition.Conditions != null ? definition.Conditions.Count : 0;
		while (state.conditionProgress.Count < targetCount)
		{
			state.conditionProgress.Add(0);
		}

		if (state.conditionProgress.Count > targetCount)
		{
			state.conditionProgress.RemoveRange(targetCount, state.conditionProgress.Count - targetCount);
		}

		return state;
	}

	private void RecordEvent(AchievementEventData achievementEvent)
	{
		if (!HasDefinitions || achievementEvent == null)
		{
			return;
		}

		bool anyChanged = false;
		List<AchievementDefinition> completedDefinitions = new List<AchievementDefinition>();
		foreach (var definition in GetDefinitions())
		{
			if (definition == null) continue;

			AchievementProgressState state = EnsureState(definition);
			if (state.completed) continue;

			bool definitionChanged = false;
			for (int i = 0; i < definition.Conditions.Count; i++)
			{
				var condition = definition.Conditions[i];
				if (condition == null || !condition.Matches(achievementEvent))
				{
					continue;
				}

				int before = state.conditionProgress[i];
				int after = Mathf.Clamp(before + condition.GetProgressIncrement(achievementEvent), 0, condition.RequiredCount);
				if (after != before)
				{
					state.conditionProgress[i] = after;
					definitionChanged = true;
				}
			}

			if (!definitionChanged)
			{
				continue;
			}

			anyChanged = true;
			if (IsDefinitionComplete(definition, state))
			{
				state.completed = true;
				state.completionOrder = nextCompletionOrder++;
				completedDefinitions.Add(definition);
			}
		}

		if (!anyChanged)
		{
			return;
		}

		if (completedDefinitions.Count > 0)
		{
			RebuildUnlockedRewards();
		}

		Save();

		foreach (var definition in completedDefinitions)
		{
			OnAchievementCompleted?.Invoke(definition);
		}
	}

	private bool IsDefinitionComplete(AchievementDefinition definition, AchievementProgressState state)
	{
		if (definition.Conditions == null || definition.Conditions.Count == 0)
		{
			return true;
		}

		for (int i = 0; i < definition.Conditions.Count; i++)
		{
			var condition = definition.Conditions[i];
			if (condition == null)
			{
				continue;
			}

			if (state.conditionProgress[i] < condition.RequiredCount)
			{
				return false;
			}
		}

		return true;
	}

	private void RebuildUnlockedRewards()
	{
		unlockedShopEquipablesByPool.Clear();
		unlockedChestEquipablesByPool.Clear();
		unlockedSkillsByPool.Clear();

		foreach (var definition in GetDefinitions())
		{
			if (definition == null || !IsCompleted(definition))
			{
				continue;
			}

			foreach (var reward in definition.Rewards)
			{
				switch (reward)
				{
					case UnlockShopEquipableAchievementRewardData shopReward:
						AddUnlockedEquipable(unlockedShopEquipablesByPool, shopReward.TargetPool, shopReward.Equipable);
						break;
					case UnlockChestEquipableAchievementRewardData chestReward:
						AddUnlockedEquipable(unlockedChestEquipablesByPool, chestReward.TargetPool, chestReward.Equipable);
						break;
					case UnlockHeroSpiritSkillAchievementRewardData skillReward:
						AddUnlockedSkill(skillReward.TargetPool, skillReward.Skill);
						break;
				}
			}
		}
	}

	private static void AddUnlockedEquipable(
		Dictionary<string, List<EquipableBaseData>> target,
		UnlockableEquipablePool pool,
		EquipableBaseData equipable)
	{
		if (pool == null || equipable == null)
		{
			return;
		}

		if (!target.TryGetValue(pool.PoolId, out List<EquipableBaseData> list))
		{
			list = new List<EquipableBaseData>();
			target[pool.PoolId] = list;
		}

		if (!list.Contains(equipable))
		{
			list.Add(equipable);
		}
	}

	private void AddUnlockedSkill(UnlockableSkillPool pool, BaseSkillData skill)
	{
		if (pool == null || skill == null)
		{
			return;
		}

		if (!unlockedSkillsByPool.TryGetValue(pool.PoolId, out List<BaseSkillData> list))
		{
			list = new List<BaseSkillData>();
			unlockedSkillsByPool[pool.PoolId] = list;
		}

		if (!list.Contains(skill))
		{
			list.Add(skill);
		}
	}

	private static List<EquipableBaseData> GetUnlockedEquipables(
		Dictionary<string, List<EquipableBaseData>> target,
		string poolId)
	{
		if (!string.IsNullOrWhiteSpace(poolId) && target.TryGetValue(poolId, out List<EquipableBaseData> list))
		{
			return list;
		}

		return null;
	}

	private static void AddDistinct<T>(List<T> destination, IEnumerable<T> source) where T : class
	{
		if (destination == null || source == null)
		{
			return;
		}

		foreach (var item in source)
		{
			if (item == null || destination.Contains(item))
			{
				continue;
			}

			destination.Add(item);
		}
	}
}
