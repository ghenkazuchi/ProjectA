using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyService
{
	private readonly PlayerWallet _wallet;
	private readonly IWalletStorage storage;
	public PlayerWallet Wallet => _wallet;

	public CurrencyService(IWalletStorage storage)
	{
		this.storage = storage;
		_wallet = new PlayerWallet();
		var dto = storage.Load();
		_wallet.FromDTO(dto);
	}
	public bool TrySpend(CurrencyType type,int amount)
	{
		bool ok = _wallet.TrySpend(type, amount);
		if (ok)
		{
			storage.Save(_wallet.ToDTO());
		}
		return ok;
	}
	public void Add(CurrencyType type, int amount)
	{
		_wallet.Adð(type, amount);
		storage.Save(_wallet.ToDTO());
	}
	public bool HasEnough(CurrencyType type, int amount)
	{
		return _wallet.CanAfford(type, amount);
	}

}
