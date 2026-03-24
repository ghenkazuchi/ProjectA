using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteSheetPlayer : MonoBehaviour
{
	[SerializeField] private Image img;
	[SerializeField] private CanvasGroup canvasGroup;

	[SerializeField] private Sprite[] frames;
	[SerializeField] private float fps = 30f;

	private void Awake()
	{
		if (img == null)
			img = GetComponent<Image>();
		if (canvasGroup == null)
			canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = gameObject.AddComponent<CanvasGroup>();

		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;
	}

	public IEnumerator PlayOnce(float endFade = 0.25f)
	{
		if (img == null || frames == null || frames.Length == 0)
		{
			HideImmediate();
			yield break;
		}

		canvasGroup.alpha = 1f;
		float dt = 1f / Mathf.Max(1f, fps);

		for (int i = 0; i < frames.Length; i++) 
		{
			img.sprite = frames[i];
			yield return new WaitForSeconds(dt);
		}

		if(endFade > 0f) yield return canvasGroup.DOFade(0f,endFade).SetEase(Ease.InQuad).WaitForCompletion();
		canvasGroup.alpha = 0f;
	}
	public void SetVFX(Sprite[] newFrames, float? newFps = null)
	{
		if (newFrames != null && newFrames.Length > 0)
			frames = newFrames;
		if (newFps.HasValue && newFps.Value > 0f)
			fps = newFps.Value;
	}

	public void HideImmediate()
	{
		if (canvasGroup != null)
			canvasGroup.alpha = 0f;
	}

}
