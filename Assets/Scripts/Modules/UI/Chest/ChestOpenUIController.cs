using DG.Tweening;
using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestOpenUIController : Singleton<ChestOpenUIController>, IMessageHandle
{
	[SerializeField] ChestItemSlot[] chestItemSlots;
	[SerializeField] CanvasGroup UICanvasGroup;
	[SerializeField] PartyMemberSelectionUI partyMemberSelectionUI;
	private ChestLootEntry selectedItem;

	[Header("Animation")]
	[SerializeField] Sprite[] frames;
	[SerializeField] RectTransform chestRect;
	[SerializeField] CanvasGroup chestCanvasGroup;
	[SerializeField] Image chestImage;
	[SerializeField] Vector2 chestIntendedPos = new Vector2(0, 0);
	private Vector2 originalChestPos;
	[SerializeField] CanvasGroup selectItemCanvasGroup;
	private void Awake()
	{
		UICanvasGroup.alpha = 0f;
		UICanvasGroup.interactable = false;
		UICanvasGroup.blocksRaycasts = false;
		originalChestPos = chestRect.anchoredPosition;
		chestCanvasGroup.alpha = 0f;
		chestCanvasGroup.blocksRaycasts = false;
		chestCanvasGroup.interactable = false;
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnChestOpen:
				if (message.data != null && message.data.Length > 0 && message.data[0] is List<ChestLootEntry> entries)
				{
					PlayOpenChestAnimation();
					InitializeChestItems(entries);
					OpenChestUI();
				}
				break;
			case MessageType.OnItemSelected:
				Debug.Log("Clicked");
				if (message.data != null && message.data.Length > 0 && message.data[0] is ChestLootEntry selectedEntry)
				{
					Debug.Log("Showing Party Selection");
					ShowPartySelection(selectedEntry);
				}
				break;
		}
	}

	public void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnChestOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnItemSelected, this);
	}
	public void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnChestOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnItemSelected, this);
	}
	public void OpenChestUI()
	{
		UICanvasGroup.alpha = 1f;
		UICanvasGroup.interactable = true;
		UICanvasGroup.blocksRaycasts = true;
	}
	public void OpenItemPanel()
	{
		selectItemCanvasGroup.alpha = 1f;
		selectItemCanvasGroup.interactable = true;
		selectItemCanvasGroup.blocksRaycasts = true;
	}

	public void CloseItemPanel()
	{
		selectItemCanvasGroup.alpha = 0f;
		selectItemCanvasGroup.interactable = false;
		selectItemCanvasGroup.blocksRaycasts = false;
	}
	public void InitializeChestItems(List<ChestLootEntry> entries)
	{
		for (int i = 0; i < chestItemSlots.Length; i++)
		{
			if (i < entries.Count)
			{
				chestItemSlots[i].SetItemData(entries[i]);
			}
			else
			{
				chestItemSlots[i].ClearItemSlot();
			}
		}
	}
	public void ShowPartySelection(ChestLootEntry entry)
	{
		selectedItem = entry;
		if (partyMemberSelectionUI != null)
		{
			partyMemberSelectionUI.ShowPartySelection(entry);
		}
		else
		{
			Debug.LogError("PartyMemberSelectionUI is not assigned!");
		}
	}
	public void CloseChestUI()
	{
		UICanvasGroup.alpha = 0f;
		UICanvasGroup.interactable = false;
		UICanvasGroup.blocksRaycasts = false;
		foreach (ChestItemSlot slot in chestItemSlots)
		{
			slot.ClearItemSlot();
		}
		selectedItem = default;
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInteractEnd));
		MessageManager.Instance.SendMessage(new Message(MessageType.OnChestClose));
		CloseItemPanel();
		chestRect.anchoredPosition = originalChestPos;
		chestImage.sprite = frames[0];
	}

	private void PlayOpenChestAnimation()
	{
		UICanvasGroup.alpha = 1f;
		UICanvasGroup.interactable = false;
		UICanvasGroup.blocksRaycasts = true;
		chestCanvasGroup.alpha = 1f;

		Sequence sequence = DOTween.Sequence().SetLink(gameObject);
		sequence.Append(chestRect.DOAnchorPos(chestIntendedPos, 0.75f).SetEase(Ease.OutBounce));
		sequence.Append(DOVirtual.Int(0, frames.Length - 1, 0.35f, i => chestImage.sprite = frames[i]));
		sequence.AppendCallback(() => SpawnChestAnimation());
		sequence.OnComplete(() =>
		{
			UICanvasGroup.interactable = true;
			UICanvasGroup.blocksRaycasts = true;
			chestCanvasGroup.blocksRaycasts = false;
			chestCanvasGroup.interactable = false;
			chestCanvasGroup.alpha = 0f;
		});

	}
	private void SpawnChestAnimation()
	{
		OpenItemPanel();
		var chestParent = chestRect.parent as RectTransform;
		for (int i = 0; i < chestItemSlots.Length; i++)
		{
			var slot = chestItemSlots[i];

			var slotRect = slot.GetComponent<RectTransform>();
			var slotParent = slotRect.parent as RectTransform;

			Vector2 endAnchored = slotRect.anchoredPosition;

			Vector2 chestWorld = chestRect.TransformPoint(chestRect.rect.center);
			Vector2 chestLocal;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				slotParent, RectTransformUtility.WorldToScreenPoint(null, chestWorld), null, out chestLocal
			);

			slotRect.anchoredPosition = chestLocal;
			slotRect.localScale = Vector3.zero;

			float t = 0.55f;
			float delay = i * 0.08f;

			slotRect.DOAnchorPos(endAnchored, t).SetEase(Ease.OutQuad).SetDelay(delay).SetLink(slot.gameObject);
			slotRect.DOScale(1f, t).SetEase(Ease.OutBack).SetDelay(delay).SetLink(slot.gameObject);
		}
	}
}
