using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public BattleDialogBox battleDialogBox;
    [SerializeField] private TurnTimelineUI timelineUI;
    public BattleHud currentPlayerCharacterInfo;

    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public void UpdateTimelineUI(List<EntityBase> upcomingEntities)
    {
        if (timelineUI != null)
        {
            timelineUI.UpdateTimeline(upcomingEntities);
        }
    }

    public IEnumerator TypeDialog(string text, bool autoShowTurnEntityInfo = true)
    {
        if (battleDialogBox != null)
        {
            yield return battleDialogBox.TypeDialog(text, autoShowTurnEntityInfo);
        }
    }
}
