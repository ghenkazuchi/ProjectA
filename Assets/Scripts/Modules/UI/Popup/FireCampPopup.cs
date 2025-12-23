using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireCampPopup : MonoBehaviour
{
	public CanvasGroup canvasgroup;
	public Button yesButton;
	public Button noButton;

	private Action<bool> currentCallBack;

	private void Awake()
	{
		if (yesButton) yesButton.onClick.AddListener(() => Close(true));
		if (noButton) noButton.onClick.AddListener(() => Close(false));
		HideImmediate();
	}


	private void Close(bool Result)
	{
		var cb = currentCallBack;
		currentCallBack = null;
		cb?.Invoke(Result);
		HideImmediate();
	}

	public void Show(Action<bool> cb)
	{
		currentCallBack = cb;
		canvasgroup.alpha = 1f;
		canvasgroup.interactable = true;
		canvasgroup.blocksRaycasts = true;
	}

	public void HideImmediate()
	{
		canvasgroup.alpha = 0f;
		canvasgroup.interactable = false;
		canvasgroup.blocksRaycasts = false;
		currentCallBack = null;
	}
}
