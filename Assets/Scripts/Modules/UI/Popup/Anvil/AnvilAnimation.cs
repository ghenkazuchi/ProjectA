using DG.Tweening;
using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnvilAnimation : MonoBehaviour
{
	[Header("Groups")]
	[SerializeField] private CanvasGroup fatherCanvasGroup;
	[SerializeField] private CanvasGroup upgreadeAnimationCanvasGroup; 
	[SerializeField] private CanvasGroup resultCanvasGroup;

	[Header("Impact Frames")]
	[SerializeField] private Image animationImage;  
	[SerializeField] private Sprite[] frames;       

	[Header("Result UI")]
	[SerializeField] private Transform containerTransform;
	[SerializeField] private Image resultItemIcon;
	[SerializeField] private Item resultItem;

	[Header("Timings")]
	[SerializeField] private float framesDuration = 0.45f;
	[SerializeField] private float resultPopDuration = 0.32f;
	[SerializeField] private float holdResultDuration = 1.1f;
	[SerializeField] private float fadeOutDuration = 0.25f;

	[Header("Punch/FX")]
	[SerializeField] private Vector3 punchScale = new Vector3(0.15f, 0.15f, 0f);
	[SerializeField] private int punchVibrato = 8;
	[SerializeField] private float punchElasticity = 0.9f;

	private Sequence playingSeq;
	private Color _iconOriginalColor;

	private void Awake()
	{
		fatherCanvasGroup.alpha = 0f;
		upgreadeAnimationCanvasGroup.alpha = 0f;
		resultCanvasGroup.alpha = 0f;

		if (!containerTransform && resultItemIcon)
			containerTransform = resultItemIcon.transform.parent;

		if (resultItemIcon)
			_iconOriginalColor = resultItemIcon.color;
	}
	public void PlayAnimation()
	{
		DOTween.Kill(gameObject, complete: false);
		playingSeq?.Kill();
		fatherCanvasGroup.alpha = 1f;
		upgreadeAnimationCanvasGroup.alpha = 1f;
		resultCanvasGroup.alpha = 0f;

		if (containerTransform) containerTransform.localScale = Vector3.one;
		if (resultItemIcon) resultItemIcon.color = new Color(_iconOriginalColor.r, _iconOriginalColor.g, _iconOriginalColor.b, 0f);

		playingSeq = DOTween.Sequence().SetLink(gameObject);

		if (frames != null && frames.Length > 0 && animationImage)
		{
			playingSeq.Append(
				DOVirtual.Int(0, frames.Length - 1, framesDuration, i =>
				{
					if (i >= 0 && i < frames.Length) animationImage.sprite = frames[i];
				}).SetEase(Ease.Linear)
			);
		}
		else
		{
			playingSeq.AppendInterval(0.2f);
		}

		playingSeq.AppendCallback(ShowResult);

		playingSeq.AppendInterval(holdResultDuration);
		playingSeq.Append(fatherCanvasGroup.DOFade(0f, fadeOutDuration));
		playingSeq.Join(resultCanvasGroup.DOFade(0f, fadeOutDuration));
		playingSeq.AppendCallback(() =>
		{
			upgreadeAnimationCanvasGroup.alpha = 0f;
			//OnAnimationComplete?.Invoke();
			MessageManager.Instance.SendMessage(new Message(MessageType.OnAnvilPopupClose));
		});
	}

	private void ShowResult()
	{
		upgreadeAnimationCanvasGroup.DOFade(0f, 0.1f);
		resultCanvasGroup.alpha = 1f;
		resultCanvasGroup.DOFade(1f, 0.1f);

		if (resultItemIcon && resultItem != null && resultItem.itemBaseData != null)
		{
			resultItemIcon.sprite = resultItem.itemBaseData.icon;
			resultItemIcon.color = resultItem.itemBaseData.GetTint(resultItem.currentItemGrade);
																								
			var finalTint = resultItemIcon.color;
			resultItemIcon.color = Color.white;
			resultItemIcon.DOColor(finalTint, 0.18f);
			resultItemIcon.DOFade(1f, 0.12f).From(0f);
		}

		if (containerTransform)
		{
			containerTransform.localScale = Vector3.one * 0.01f;
			var pop = containerTransform.DOScale(1f, resultPopDuration)
										.SetEase(Ease.OutBack);
			var punch = containerTransform.DOPunchScale(punchScale, 0.28f, punchVibrato, punchElasticity);
			DOTween.Sequence().SetLink(gameObject).Append(pop).Join(punch);
		}
	}
	public void SetResult(Item item) => resultItem = item;
	public void SetFrames(Sprite[] impactFrames) => frames = impactFrames;
}
