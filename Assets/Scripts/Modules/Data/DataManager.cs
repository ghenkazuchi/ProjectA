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
		var achievementStorage = new JSonAchievementStorage("achievements.json");
		Achievements = new AchievementService(LoadAchievementDatabase(), achievementStorage);
		_ = AchievementToastController.Instance;
		_ = AchievementScreenController.Instance;
		Currency.Add(CurrencyType.Gold, 20);
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
	public void Handle(Message message)
    {
        throw new System.NotImplementedException();
    }
}
