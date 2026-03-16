using System.Collections.Generic;
using UnityEngine;

public class AchievementToastController : HaKien.Singleton<AchievementToastController>
{
	private sealed class ToastItem
	{
		public string title;
		public string body;
		public float expireAt;
	}

	private readonly Queue<ToastItem> pendingToasts = new Queue<ToastItem>();
	private ToastItem currentToast;
	private AchievementService subscribedService;

	private void Update()
	{
		TrySubscribe();

		if (currentToast != null && Time.unscaledTime >= currentToast.expireAt)
		{
			currentToast = null;
		}

		if (currentToast == null && pendingToasts.Count > 0)
		{
			currentToast = pendingToasts.Dequeue();
			currentToast.expireAt = Time.unscaledTime + 4f;
		}
	}

	private void OnGUI()
	{
		if (currentToast == null)
		{
			return;
		}

		Rect rect = new Rect(Screen.width - 380f, 20f, 360f, 120f);
		GUI.Box(rect, "Achievement Unlocked");
		GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 28f, rect.width - 24f, rect.height - 36f));
		GUILayout.Label(currentToast.title);
		GUILayout.Space(8f);
		GUILayout.Label(currentToast.body);
		GUILayout.EndArea();
	}

	private void OnDestroy()
	{
		Unsubscribe();
	}

	private void TrySubscribe()
	{
		AchievementService service = DataManager.Instance != null ? DataManager.Instance.Achievements : null;
		if (service == null || service == subscribedService)
		{
			return;
		}

		Unsubscribe();
		subscribedService = service;
		subscribedService.OnAchievementCompleted += HandleAchievementCompleted;
	}

	private void Unsubscribe()
	{
		if (subscribedService == null)
		{
			return;
		}

		subscribedService.OnAchievementCompleted -= HandleAchievementCompleted;
		subscribedService = null;
	}

	private void HandleAchievementCompleted(AchievementDefinition definition)
	{
		if (definition == null)
		{
			return;
		}

		List<string> rewardLines = DataManager.Instance.Achievements.GetRewardSummaries(definition);
		string body = rewardLines.Count > 0
			? string.Join("\n", rewardLines)
			: definition.Description;

		pendingToasts.Enqueue(new ToastItem
		{
			title = definition.AchievementTitle,
			body = body
		});
	}
}
