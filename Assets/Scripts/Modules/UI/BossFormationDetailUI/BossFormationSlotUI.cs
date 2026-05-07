using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BossFormationSlotUI : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private Image monsterPortrait;
	[SerializeField] private Member currentMonsterData;
	
	private Action<Member> onClickCallback;

	public void Init(Member memberData, Action<Member> onClick)
	{
		currentMonsterData = memberData;
		onClickCallback = onClick;

		if (currentMonsterData != null && currentMonsterData.monster != null)
		{
			monsterPortrait.enabled = true;
			monsterPortrait.sprite = currentMonsterData.monster.EntityPortrait;
		}
		else
		{
			monsterPortrait.enabled = false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		onClickCallback?.Invoke(currentMonsterData);
	}
}
