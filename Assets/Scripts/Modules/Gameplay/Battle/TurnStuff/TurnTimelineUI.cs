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

	public void HandleTimeLineChanged(EntityBase current,List<EntityBase> upcoming, int version)
	{
		if(version <= lastVersion) return;
		lastVersion = version;
		UpdateTimeline(upcoming);
	}

	//private IEnumerator PlayUpdate(EntityBase current, List<EntityBase> list)
	//{
	//	if (turnIcons.Count > 0 && list.Count > 0 && list[0] == current)
	//		//yield return turnIcons[0].PlayConsume();
	//}
	public void UpdateTimeline(List<EntityBase> upcomingEntities)
	{
		for (int i = 0; i < turnIcons.Count; i++)
		{
			if (i < upcomingEntities.Count)
			{
				turnIcons[i].SetData(upcomingEntities[i]);
			}
		}
	}
}
