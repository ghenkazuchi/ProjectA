using System.Collections;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class DataManager : Singleton<DataManager>, IMessageHandle
{
   public CurrencyService Currency { get; private set; }
   public AchievementService Achievements { get; private set; }
	[SerializeField] private AchievementDatabase achievementDatabase;

	private void Awake()
	{
		var storage = new JSonWalletStorage("wallet.json");
		Currency = new CurrencyService(storage);
		
		// Wipe the wallet at the start of every Editor session/run
		Currency.Clear();

		var achievementStorage = new JSonAchievementStorage("achievements.json");
		Achievements = new AchievementService(LoadAchievementDatabase(), achievementStorage);
		_ = AchievementToastController.Instance;
		_ = AchievementScreenController.Instance;
	}

	private AchievementDatabase LoadAchievementDatabase()
	{
		if (achievementDatabase != null)
		{
			return achievementDatabase;
		}

		achievementDatabase = Resources.Load<AchievementDatabase>("AchievementDatabase");
		if (achievementDatabase == null)
		{
			achievementDatabase = Resources.Load<AchievementDatabase>("Achievements/AchievementDatabase");
		}

		return achievementDatabase;
	}
	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnCharacterCreationEnter, this);
	}

	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnCharacterCreationEnter, this);
	}

	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnCharacterCreationEnter:
				Currency?.Clear();
				break;
		}
	}
}
