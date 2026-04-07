using HaKien;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple overlay hint that appears during portal selection mode.
/// Shows instructions like "Click on a discovered portal to teleport. Press Tab or Escape to cancel."
/// </summary>
public class PortalSelectionUI : MonoBehaviour, IMessageHandle
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private TextMeshProUGUI hintText;

	private void Awake()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnPortalOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPortalClose, this);
	}

	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPortalOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPortalClose, this);
	}

	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnPortalOpen:
				Show();
				break;
			case MessageType.OnPortalClose:
				Hide();
				break;
		}
	}

	private void Show()
	{
		if (hintText != null)
		{
			int discovered = PortalManager.Instance != null
				? PortalManager.Instance.GetDiscoveredPortals().Count
				: 0;
			hintText.text = discovered > 1
				? "Click on a discovered portal to teleport\nPress Tab or Escape to cancel"
				: "No other discovered portals yet\nPress Tab or Escape to cancel";
		}

		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = false; // Don't block mouse clicks on portals!
		}
	}

	private void Hide()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}
}
