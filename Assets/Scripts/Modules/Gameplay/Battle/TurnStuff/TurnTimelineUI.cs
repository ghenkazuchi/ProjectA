using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimelineUI : MonoBehaviour
{
	[SerializeField] private List<TurnIconUI> turnIcons;

	private int lastVersion = -1;
	private Coroutine queue;

	private void OnEnable()
	{
		TimelineManager.Instance.OnTimeLineChanged += HandleTimeLineChanged;
	}
	private void OnDisable()
	{
		TimelineManager.Instance.OnTimeLineChanged -= HandleTimeLineChanged;
	}

	public void HandleTimeLineChanged(EntityBase current, List<EntityBase> upcoming, int version)
	{
		if (version <= lastVersion) return;
		lastVersion = version;
		UpdateTimeline(upcoming);
	}

	public void UpdateTimeline(List<EntityBase> upcomingEntities)
	{
		for (int i = 0; i < turnIcons.Count; i++)
		{
			if (i < upcomingEntities.Count)
			{
				turnIcons[i].gameObject.SetActive(true);
				turnIcons[i].SetData(upcomingEntities[i]);
			}
			else
			{
				turnIcons[i].gameObject.SetActive(false);
			}
		}
	}
}
