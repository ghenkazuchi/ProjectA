using HaKien;
using System.Collections.Generic;
using UnityEngine;

public class AchievementScreenController : Singleton<AchievementScreenController>, IMessageHandle
{
	private bool isOpen;
	private Vector2 listScroll;
	private Vector2 detailScroll;
	private string selectedAchievementId;

	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnAchievementScreenOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnAchievementScreenClose, this);
	}

	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAchievementScreenOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAchievementScreenClose, this);
	}

	private void Update()
	{
		if (!isOpen)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnAchievementScreenClose));
		}
	}

	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnAchievementScreenOpen:
				isOpen = true;
				EnsureSelection();
				break;
			case MessageType.OnAchievementScreenClose:
				isOpen = false;
				break;
		}
	}

	private void OnGUI()
	{
		if (!isOpen)
		{
			return;
		}

		GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);

		float panelX = 30f;
		float panelY = 30f;
		float panelWidth = Screen.width - 60f;
		float panelHeight = Screen.height - 60f;
		float listWidth = Mathf.Min(420f, panelWidth * 0.38f);
		float detailWidth = panelWidth - listWidth - 20f;

		GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "Achievements");

		DrawAchievementList(new Rect(panelX + 10f, panelY + 35f, listWidth, panelHeight - 45f));
		DrawAchievementDetails(new Rect(panelX + listWidth + 20f, panelY + 35f, detailWidth - 10f, panelHeight - 45f));
	}

	private void DrawAchievementList(Rect rect)
	{
		GUILayout.BeginArea(rect, GUI.skin.box);
		listScroll = GUILayout.BeginScrollView(listScroll);

		foreach (var definition in GetDefinitions())
		{
			if (definition == null)
			{
				continue;
			}

			bool completed = Service != null && Service.IsCompleted(definition);
			bool hidden = definition.HiddenUntilCompleted && !completed;
			string label = hidden
				? "Hidden Achievement"
				: $"{definition.AchievementTitle} {(completed ? "[Done]" : "")}";

			if (GUILayout.Button(label))
			{
				selectedAchievementId = definition.AchievementId;
			}
		}

		GUILayout.EndScrollView();
		if (GUILayout.Button("Close"))
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnAchievementScreenClose));
		}
		GUILayout.EndArea();
	}

	private void DrawAchievementDetails(Rect rect)
	{
		AchievementDefinition selected = GetSelectedAchievement();
		GUILayout.BeginArea(rect, GUI.skin.box);
		detailScroll = GUILayout.BeginScrollView(detailScroll);

		if (selected == null)
		{
			GUILayout.Label("No achievements available.");
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			return;
		}

		bool completed = Service != null && Service.IsCompleted(selected);
		bool hidden = selected.HiddenUntilCompleted && !completed;

		GUILayout.Label(hidden ? "Hidden Achievement" : selected.AchievementTitle);
		GUILayout.Space(8f);
		GUILayout.Label(hidden ? "Complete this achievement to reveal its full details." : selected.Description);
		GUILayout.Space(12f);
		GUILayout.Label(completed ? "Status: Completed" : "Status: In Progress");
		GUILayout.Space(8f);
		GUILayout.Label("Conditions");
		for (int i = 0; i < selected.Conditions.Count; i++)
		{
			var condition = selected.Conditions[i];
			if (condition == null)
			{
				continue;
			}

			int progress = Service != null ? Service.GetConditionProgress(selected, i) : 0;
			int clampedProgress = Mathf.Clamp(progress, 0, condition.RequiredCount);
			string conditionText = hidden
				? "Hidden condition"
				: $"{condition.GetDescription()} ({clampedProgress}/{condition.RequiredCount})";
			GUILayout.Label(conditionText);
		}

		GUILayout.Space(12f);
		GUILayout.Label("Rewards");
		List<string> rewards = Service != null ? Service.GetRewardSummaries(selected) : new List<string>();
		if (rewards.Count == 0)
		{
			GUILayout.Label("No rewards configured.");
		}
		else
		{
			foreach (string reward in rewards)
			{
				GUILayout.Label(hidden ? "Hidden reward" : reward);
			}
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private AchievementDefinition GetSelectedAchievement()
	{
		EnsureSelection();
		foreach (var definition in GetDefinitions())
		{
			if (definition != null && definition.AchievementId == selectedAchievementId)
			{
				return definition;
			}
		}

		return null;
	}

	private void EnsureSelection()
	{
		if (!string.IsNullOrWhiteSpace(selectedAchievementId))
		{
			return;
		}

		foreach (var definition in GetDefinitions())
		{
			if (definition == null) continue;
			selectedAchievementId = definition.AchievementId;
			break;
		}
	}

	private IReadOnlyList<AchievementDefinition> GetDefinitions()
	{
		return Service != null ? Service.GetDefinitions() : System.Array.Empty<AchievementDefinition>();
	}

	private AchievementService Service => DataManager.Instance != null ? DataManager.Instance.Achievements : null;
}
