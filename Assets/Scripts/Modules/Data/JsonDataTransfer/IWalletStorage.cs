using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWalletStorage
{
	PlayerWallet.WalletDTO Load();
	void Save(PlayerWallet.WalletDTO walletDTO);
}
