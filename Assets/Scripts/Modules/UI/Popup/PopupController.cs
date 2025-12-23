using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupController : Singleton<PopupController>, IMessageHandle
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private FireCampPopup fireCampPopup;
	[SerializeField] private AnvilPopup anvilPopup;
	[SerializeField] private ShopUIManager shopUIManager;
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnFireCampPopupOpen:
				{
					Show();
					Action<bool> cb = null;
					if (message.data != null && message.data.Length > 0)
						cb = message.data[0] as Action<bool>;

					if (fireCampPopup != null)
					{
						fireCampPopup.Show(result =>
						{
							cb?.Invoke(result);
							MessageManager.Instance.SendMessage(new Message(MessageType.OnFireCampPopupClose));
						});
					}
					break;
				}
			case MessageType.OnFireCampPopupClose:
				Hide();
				fireCampPopup?.HideImmediate();
				break;
			case MessageType.OnAnvilPopupOpen:
				Show();
				anvilPopup?.Show();
				break;
			case MessageType.OnAnvilPopupClose:
				Hide();
				anvilPopup?.Hide();
				MessageManager.Instance.SendMessage(new Message(MessageType.OnInteractEnd));
				break;
			case MessageType.OnSelectCharacterReturn:
				anvilPopup.Hide();
				anvilPopup.Show();
				break;
			case MessageType.OnShopOpen:
				Show();
				shopUIManager?.Show();
				break;
			case MessageType.OnShopClose:
				Hide();
				shopUIManager?.Hide();
				break;

		}
	}

	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnFireCampPopupOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnAnvilPopupOpen,this);
		MessageManager.Instance.AddSubcriber(MessageType.OnAnvilPopupClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnSelectCharacterReturn, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopOpen, this);

	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnFireCampPopupOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAnvilPopupOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAnvilPopupClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnSelectCharacterReturn,this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopOpen, this);
	}
	private void Awake()
	{
		Hide();
	}
	private void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

	}
	private void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}
}
