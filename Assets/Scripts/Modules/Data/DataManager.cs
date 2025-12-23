using System.Collections;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class DataManager : Singleton<DataManager>, IMessageHandle
{
   public CurrencyService Currency { get; private set; }

	private void Awake()
	{
		var storage = new JSonWalletStorage("wallet.json");
		Currency = new CurrencyService(storage);
		Currency.Add(CurrencyType.Gold, 20);
	}
	public void Handle(Message message)
    {
        throw new System.NotImplementedException();
    }
}
