using DG.Tweening.Core.Easing;
using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopKeeperInteractableObject : Interacable, IShopKeeper
{
	[Header("Shop stuff")]
	[SerializeField] private List<EquipableBaseData> shopPool;
	[SerializeField] private UnlockableEquipablePool unlockableShopPool;
	[SerializeField] private int stockSize = 9;

	private List<EquipableBaseData> _stock = new List<EquipableBaseData>();
	public void InitiateEquipables()
	{
		_stock.Clear();
		RestockEquipables();
	}

	public void RestockEquipables()
	{
		_stock = PickRandom(GetResolvedShopPool(), stockSize);
		MessageManager.Instance.SendMessage(new Message(MessageType.OnShopUpdated, new object[] { new List<EquipableBaseData>(_stock) }));
	}

	public override void TriggerInteraction()
	{
		if(_stock.Count == 0) InitiateEquipables();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnShopOpen, new object[] {new List<EquipableBaseData>(_stock),this} ));
	}
	private List<T> PickRandom<T>(List<T> list, int size)
	{
		var result = new List<T>();
		if (list == null || list.Count == 0) return result;
		var tmp = new List<T>(list);
		int pickCount = Mathf.Min(size, tmp.Count);
		for (int i = 0; i < pickCount; i++)
		{
			int idx = Random.Range(0, tmp.Count);
			var pick = tmp[idx];
			result.Add(pick);
			tmp.RemoveAt(idx);
		}
		return result;
	}
	public bool TryPurchase(ShopReplaceSelection selection, int price)
	{
		if (selection.target == null || selection.newEquip == null)
			return false;

		var equip = selection.newEquip;

		var currency = DataManager.Instance.Currency;

		if (!currency.TrySpend(CurrencyType.Gold, price))
		{
			Debug.Log("Not enough gold");
			return false;
		}
		selection.target.ApplyReplaceSelection(selection.removeWeapon, selection.removeItemIndices);
		bool equipOk = false;
		if (equip is WeaponBaseData wb)
		{
			equipOk = selection.target.TryEquipWeapon(new Weapon(wb));
		}
		else if (equip is ItemBaseData ib)
		{
			equipOk = selection.target.TryAddItem(new Item(ib, ItemGrade.Normal));
		}

		if (!equipOk)
		{
			Debug.LogError("Equip failed after replacement");
			currency.Add(CurrencyType.Gold, price);
			return false;
		}
		_stock.Remove(equip);
		GameEventBus.Publish(new ShopPurchaseEvent { Equipable = equip });
		return true;
	}

	private List<EquipableBaseData> GetResolvedShopPool()
	{
		if (unlockableShopPool == null || DataManager.Instance?.Achievements == null)
		{
			return new List<EquipableBaseData>(shopPool);
		}

		return DataManager.Instance.Achievements.GetEquipablesForPool(unlockableShopPool, shopPool);
	}

}
