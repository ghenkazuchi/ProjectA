using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class AchievementProgressFileDTO
{
	public int nextCompletionOrder = 1;
	public List<AchievementProgressEntryDTO> achievements = new List<AchievementProgressEntryDTO>();
}

[Serializable]
public class AchievementProgressEntryDTO
{
	public string achievementId;
	public bool completed;
	public int completionOrder;
	public List<int> conditionProgress = new List<int>();
}

public interface IAchievementStorage
{
	AchievementProgressFileDTO Load();
	void Save(AchievementProgressFileDTO dto);
}

public class JSonAchievementStorage : IAchievementStorage
{
	private readonly string path;

	public JSonAchievementStorage(string fileName = "achievements.json")
	{
		path = Path.Combine(Application.persistentDataPath, fileName);
	}

	public AchievementProgressFileDTO Load()
	{
		if (!File.Exists(path))
		{
			Debug.Log("Create new achievement file at: " + path);
			return new AchievementProgressFileDTO();
		}

		string json = File.ReadAllText(path);
		var dto = JsonUtility.FromJson<AchievementProgressFileDTO>(json);
		return dto ?? new AchievementProgressFileDTO();
	}

	public void Save(AchievementProgressFileDTO dto)
	{
		string json = JsonUtility.ToJson(dto, true);
		File.WriteAllText(path, json);
		Debug.Log($"[Achievements] Saved to {path}\n{json}");
	}
}
