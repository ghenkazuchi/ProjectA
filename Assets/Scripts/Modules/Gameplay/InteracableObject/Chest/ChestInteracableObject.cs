using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteracableObject : Interacable, IChestInteracable
{
	[Header("Chest Setting")]
	public List<EquipableBaseData> possibleItems = new List<EquipableBaseData>();
	[SerializeField] private UnlockableEquipablePool unlockableChestPool;
	public int itemContain = 3;

	[Header("Grade drop rate ")]
	public float normalRate;
	public float goldRate;
	public float diamondRate;

	[Header("Gold Reward")]
	[Tooltip("70% chance to drop gold instead of items")]
	[Range(0f, 1f)] public float goldDropChance = 0.7f;
	public int minGold = 10;
	public int maxGold = 50;

	private List<ChestLootEntry> generatedItems = new List<ChestLootEntry>();

	private ItemGrade RollGrade()
	{
		float r = Random.value;
		if (r < diamondRate)
			return ItemGrade.Diamond;
		else if (r < goldRate + diamondRate)
			return ItemGrade.Gold;
		else
			return ItemGrade.Normal;
	}

	public void GenerateChestItems()
	{
		generatedItems.Clear();
		List<EquipableBaseData> availableItems = GetResolvedChestPool();

		for (int i = 0; i < itemContain && availableItems.Count > 0; i++)
		{
			int randomIndex = Random.Range(0, availableItems.Count);
			var pick = availableItems[randomIndex];

			ItemGrade grade = RollGrade();
			if (pick is ItemBaseData) grade = RollGrade();

			generatedItems.Add(new ChestLootEntry(pick, grade));
		}
	}

	public void openChest()
	{
		GameEventBus.Publish(new ChestOpenEvent());

		ChestReward reward;
		if (Random.value < goldDropChance)
		{
			// Gold reward
			int gold = Random.Range(minGold, maxGold + 1);
			DataManager.Instance?.Currency?.Add(CurrencyType.Gold, gold);
			reward = ChestReward.Gold(gold);
		}
		else
		{
			// Equipable reward
			GenerateChestItems();
			reward = ChestReward.Items(generatedItems);
		}

		MessageManager.Instance.SendMessage(new Message(MessageType.OnChestOpen, new object[] { reward }));
		Destroy(gameObject);
	}

	public override void TriggerInteraction()
	{
		openChest();
	}

	private List<EquipableBaseData> GetResolvedChestPool()
	{
		if (unlockableChestPool == null || DataManager.Instance?.Achievements == null)
		{
			return new List<EquipableBaseData>(possibleItems);
		}

		return DataManager.Instance.Achievements.GetEquipablesForPool(unlockableChestPool, possibleItems);
	}
}
