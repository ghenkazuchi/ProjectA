using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
	[SerializeField] private CanvasGroup toastCanvasGroup;
	[SerializeField] private RectTransform toastTransform;
	[SerializeField] private float fadeDuration = 0.5f;
	[SerializeField] private TextMeshProUGUI toastText;

	public void Hide()
	{
		toastCanvasGroup.alpha = 0f;
		toastCanvasGroup.interactable = false;
		toastCanvasGroup.blocksRaycasts = false;
	}
	public void Show()
	{
		toastCanvasGroup.alpha = 1f;
		toastCanvasGroup.interactable = true;
		toastCanvasGroup.blocksRaycasts = true;
	}

	private Vector2 initialPos;
	private Tween currentTween;

	public void Awake()
	{
		// Store the off-screen starting position so it can bounce back
		initialPos = toastTransform.anchoredPosition;
		Hide();
	}

	public void ShowToast(string message)
	{
		toastText.text = message;
		
		// 1. Immediately kill any ongoing toast animation (fixes double-clock glitching)
		currentTween?.Kill();
		
		// 2. Reset position and visibility before starting
		toastTransform.anchoredPosition = initialPos;
		Show();

		// 3. Create the perfect Toast sequence
		Sequence sequence = DOTween.Sequence().SetLink(gameObject);
		sequence.Append(toastTransform.DOAnchorPos(Vector2.zero, fadeDuration).SetEase(Ease.OutCubic));
		sequence.AppendInterval(1.2f); // Let the player actually read the toast
		sequence.Append(toastCanvasGroup.DOFade(0f, 0.25f)); // Smooth fade out
		sequence.OnComplete(() =>
		{
			Hide();
			toastTransform.anchoredPosition = initialPos; // Reset safely behind the scenes
		});
		
		currentTween = sequence;
	}
}
