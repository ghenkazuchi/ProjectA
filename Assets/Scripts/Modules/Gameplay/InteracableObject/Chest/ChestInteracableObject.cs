using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteracableObject : Interacable, IChestInteracable
{
	[Header("Chest Setting")]
	public List<EquipableBaseData> possibleItems = new List<EquipableBaseData>();
	public int itemContain = 3;

	[Header("Grade drop rate ")]
	public float normalRate;
	public float goldRate;
	public float diamondRate;

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

		for (int i = 0; i < itemContain && possibleItems.Count > 0; i++)
		{
			int randomIndex = Random.Range(0, possibleItems.Count);
			var pick = possibleItems[randomIndex];

			ItemGrade grade = RollGrade();
			if (pick is ItemBaseData) grade = RollGrade();

			generatedItems.Add(new ChestLootEntry(pick,grade));
		}

	}

	public void openChest()
	{
		GenerateChestItems();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnChestOpen, new object[] { generatedItems }));
		Destroy(gameObject);
	}

	public override void TriggerInteraction()
	{
		openChest();
	}
}
