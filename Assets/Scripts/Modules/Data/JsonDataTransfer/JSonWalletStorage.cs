using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSonWalletStorage : IWalletStorage
{
	private readonly string path;
	public JSonWalletStorage(string fileName = "wallet.json")
	{
		path = Path.Combine(Application.persistentDataPath, fileName);
	}
	public PlayerWallet.WalletDTO Load()
	{
		if (!File.Exists(path))
		{
			Debug.Log("Create new wallet file at: " + path);	
			return new PlayerWallet.WalletDTO();
		}
		string json = File.ReadAllText(path);
		var dto = JsonUtility.FromJson<PlayerWallet.WalletDTO>(json);
		return dto;
	}

	public void Save(PlayerWallet.WalletDTO dto)
	{
		string json = JsonUtility.ToJson(dto, true);
		File.WriteAllText(path, json);
		Debug.Log($"[Wallet] Saved to {path}\n{json}");
	}
}
