using HaKien;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePoolManager : Singleton<GamePoolManager>
{
	[Header("Active Pools (set by RunProgressionManager)")]
	public MonsterPoolDatabase monsterPool;
	public RecruitabeCharacterPool recruitableCharacterPool;

	[Header("Current Game Time (set by RunProgressionManager)")]
	[SerializeField]
	public GameTime CurrentGameTime = new GameTime(GameDay.Day1, TimeOfDay.Morning);

	/// <summary>
	/// Templates that have been claimed by an interactable this session.
	/// Prevents the same character from appearing on multiple interactables.
	/// </summary>
	private HashSet<RecruitableCharacterTemplate> claimedTemplates = new HashSet<RecruitableCharacterTemplate>();

	/// <summary>
	/// Templates that the player has already recruited permanently.
	/// These will never appear again in any pool.
	/// </summary>
	private HashSet<RecruitableCharacterTemplate> recruitedTemplates = new HashSet<RecruitableCharacterTemplate>();

	/// <summary>
	/// Swaps the active pool databases. Called by RunProgressionManager when a new loop starts.
	/// </summary>
	public void SetPools(MonsterPoolDatabase monsters, RecruitabeCharacterPool recruits)
	{
		monsterPool = monsters;
		recruitableCharacterPool = recruits;
		Debug.Log($"[GamePoolManager] Pools updated. Monster: {(monsters != null ? monsters.name : "null")}, Recruit: {(recruits != null ? recruits.name : "null")}");
	}

	public void SetGameTime(GameDay day, TimeOfDay time)
	{
		CurrentGameTime = new GameTime(day, time);
		Debug.Log($"[GamePoolManager] Game time set to: {day} - {time}");
	}

	public List<MonsterSpawnData> GetCurrentMonsterPool()
	{
		if (monsterPool == null)
		{
			Debug.LogWarning("[GamePoolManager] Monster pool is null.");
			return new List<MonsterSpawnData>();
		}

		var pool = monsterPool.GetMonster(CurrentGameTime.day, CurrentGameTime.time);
		return pool;
	}

	public List<RecruitableCharacterTemplate> GetCurrentRecruitablePool()
	{
		if (recruitableCharacterPool == null)
		{
			Debug.LogWarning("[GamePoolManager] Recruit pool is null.");
			return new List<RecruitableCharacterTemplate>();
		}

		var allAvailable = recruitableCharacterPool.GetAvailableCharacters(CurrentGameTime.day, CurrentGameTime.time);

		// Filter out already-claimed and already-recruited templates
		return allAvailable
			.Where(t => !claimedTemplates.Contains(t) && !recruitedTemplates.Contains(t))
			.ToList();
	}

	/// <summary>
	/// Claims a template so no other interactable can roll it.
	/// Called when an interactable rolls its character on first interaction.
	/// </summary>
	public void ClaimTemplate(RecruitableCharacterTemplate template)
	{
		claimedTemplates.Add(template);
		Debug.Log($"[GamePoolManager] Template claimed: {template.entityData.EntityName}");
	}

	/// <summary>
	/// Permanently marks a template as recruited.
	/// The character will never appear in any pool again.
	/// </summary>
	public void MarkRecruited(RecruitableCharacterTemplate template)
	{
		recruitedTemplates.Add(template);
		Debug.Log($"[GamePoolManager] Template permanently recruited: {template.entityData.EntityName}");
	}

	/// <summary>
	/// Resets session claims. Call this when entering a new loop,
	/// so un-interacted templates can re-enter the pool.
	/// Already-recruited templates remain permanently excluded.
	/// </summary>
	public void ResetSessionClaims()
	{
		claimedTemplates.Clear();
		Debug.Log("[GamePoolManager] Session claims reset.");
	}
}
