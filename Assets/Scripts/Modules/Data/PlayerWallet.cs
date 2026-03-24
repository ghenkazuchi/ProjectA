using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerWallet
{
	public Dictionary<CurrencyType, int> amounts = new Dictionary<CurrencyType, int>();
	public event Action<CurrencyType, int> OnCurrencyChanged;

	public PlayerWallet()
	{
		foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
		{
			amounts[type] = 0;
		}
	}

	public int Get(CurrencyType type)
	{
		return amounts.TryGetValue(type, out int amount) ? amount : 0;
	}

	public bool CanAfford(CurrencyType type, int cost)
	{
		return Get(type) >= cost;
	}

	public bool TrySpend(CurrencyType type, int cost)
	{
		if (CanAfford(type, cost))
		{
			amounts[type] -= cost;
			OnCurrencyChanged?.Invoke(type, amounts[type]);
			return true;
		}
		return false;
	}

	// FIX: renamed from corrupted "Ad?" to "Add"
	public void Add(CurrencyType type, int amount)
	{
		if (amount <= 0) return;
		amounts[type] += amount;
		OnCurrencyChanged?.Invoke(type, amounts[type]);
	}

	public void Clear()
	{
		var types = new List<CurrencyType>(amounts.Keys);
		foreach (var type in types)
		{
			amounts[type] = 0;
			OnCurrencyChanged?.Invoke(type, 0);
		}
	}

	[Serializable]
	public class WalletEntryDTO
	{
		public CurrencyType type;
		public int amount;
	}

	[Serializable]
	public class WalletDTO
	{
		public List<WalletEntryDTO> entries = new List<WalletEntryDTO>();
	}

	public WalletDTO ToDTO()
	{
		var dto = new WalletDTO();
		foreach (var kvp in amounts)
		{
			dto.entries.Add(new WalletEntryDTO
			{
				type = kvp.Key,
				amount = kvp.Value
			});
		}
		return dto;
	}

	public void FromDTO(WalletDTO dto)
	{
		amounts.Clear();
		foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
		{
			amounts[type] = 0;
		}
		// FIX: was "|| dto.entries != null" which crashes when dto is null
		if (dto != null && dto.entries != null)
		{
			foreach (var e in dto.entries)
			{
				amounts[e.type] = e.amount;
			}
		}
	}
}