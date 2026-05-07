using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BossFormationController : MonoBehaviour
{
	[SerializeField] BossFormationUIDetail bossFormationUIDetail;
	[SerializeField] List<BossFormationSlotUI> slots;
	[SerializeField] CanvasGroup canvasGroup;

	private bool isShowing = false;

	private readonly Dictionary<int, GridPosition> uiIndexToGridPosition = new Dictionary<int, GridPosition>
	{
		{ 0, new GridPosition(0, 0) }, 
		{ 1, new GridPosition(0, 1) }, 
		{ 2, new GridPosition(0, 2) }, 
		{ 3, new GridPosition(1, 0) }, 
		{ 4, new GridPosition(1, 1) }, 
		{ 5, new GridPosition(1, 2) }  
	};

	private void Awake()
	{
		if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
		Hide();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			if (GameController.Instance.currentState == GameState.FreeRoam)
			{
				if (isShowing) Hide();
				else Show();
			}
			else if (isShowing)
			{
				Hide();
			}
		}
	}

	public void Show()
	{
		isShowing = true;
		if (canvasGroup)
		{
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
		else gameObject.SetActive(true);

		PopulateSlots();
	}

	public void Hide()
	{
		isShowing = false;
		if (canvasGroup)
		{
			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
		else gameObject.SetActive(false);

		if (bossFormationUIDetail != null)
			bossFormationUIDetail.Hide();
	}

	private void PopulateSlots()
	{
		var bossManager = FindAnyObjectByType<BossManager>();
		if (bossManager == null || bossManager.currentBoss == null)
		{
			foreach (var slot in slots)
			{
				slot.Init(null, OnSlotClicked);
			}
			return;
		}

		var formation = bossManager.currentBoss;

		for (int i = 0; i < slots.Count; i++)
		{
			if (uiIndexToGridPosition.TryGetValue(i, out GridPosition pos))
			{
				Member memberAtPos = formation.members.FirstOrDefault(m => m.position.Equals(pos));
				slots[i].Init(memberAtPos, OnSlotClicked);
			}
			else
			{
				slots[i].Init(null, OnSlotClicked);
			}
		}
	}

	private void OnSlotClicked(Member member)
	{
		if (member == null || member.monster == null)
		{
			if (bossFormationUIDetail != null) bossFormationUIDetail.Hide();
		}
		else
		{
			if (bossFormationUIDetail != null) bossFormationUIDetail.Show(member);
		}
	}
}
