using System.Collections.Generic;
using UnityEngine;

public class AchievementToastController : HaKien.Singleton<AchievementToastController>
{
    [SerializeField] private AchievementToastUI toastPrefab;
    [SerializeField] private RectTransform toastContainer;

    private struct PendingToast
    {
        public AchievementDefinition definition;
        public string body;
    }

    private readonly Queue<PendingToast> pendingToasts = new Queue<PendingToast>();
    private bool isToastVisible = false;
    private AchievementService subscribedService;

    private void Update()
    {
        TrySubscribe();

        if (!isToastVisible && pendingToasts.Count > 0)
        {
            ShowNextToast();
        }
    }

    private void TrySubscribe()
    {
        AchievementService service = DataManager.Instance != null ? DataManager.Instance.Achievements : null;
        if (service == null || service == subscribedService) return;

        Unsubscribe();
        subscribedService = service;
        subscribedService.OnAchievementCompleted += HandleAchievementCompleted;
    }

    private void Unsubscribe()
    {
        if (subscribedService == null) return;
        subscribedService.OnAchievementCompleted -= HandleAchievementCompleted;
        subscribedService = null;
    }

    private void OnDestroy() => Unsubscribe();

    private void HandleAchievementCompleted(AchievementDefinition definition)
    {
        if (definition == null) return;

        List<string> rewardLines = DataManager.Instance.Achievements.GetRewardSummaries(definition);
        string body = rewardLines.Count > 0 
            ? string.Join("\n", rewardLines) 
            : definition.Description;

        pendingToasts.Enqueue(new PendingToast { definition = definition, body = body });
    }

    private void ShowNextToast()
    {
        if (toastPrefab == null)
        {
            Debug.LogWarning("AchievementToastController: No toast prefab assigned!");
            pendingToasts.Dequeue();
            return;
        }

        isToastVisible = true;
        PendingToast next = pendingToasts.Dequeue();

        // Instantiate under container, or default to root if container is missing
        AchievementToastUI instance = Instantiate(toastPrefab, toastContainer);
        instance.Show(next.definition, next.body, () => 
        {
            isToastVisible = false;
        });
    }
}
