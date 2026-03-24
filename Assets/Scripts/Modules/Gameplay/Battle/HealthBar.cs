using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] Image health;
	[SerializeField] private float damageTweenDuration = 0.2f;
	[SerializeField] private Ease damageTweenEase = Ease.OutCubic;
	[SerializeField] private float healTweenDuration = 0.25f;
	[SerializeField] private Ease healTweenEase = Ease.OutQuad;
	private Tween healthTween;
	void Awake()
	{
		if (health == null)
		{
			health = GetComponentInChildren<Image>();
			if (health == null)
			{
				Debug.LogError("Health Image component not assigned or found!");
			}
		}
	}

	private void OnDisable()
	{
		healthTween?.Kill();
		healthTween = null;
	}

	public void SetHP(float hpNormalized, bool animateChange = true)
	{
		if (health == null)
		{
			return;
		}

		float targetFill = Mathf.Clamp01(hpNormalized);
		float currentFill = health.fillAmount;
		healthTween?.Kill();
		healthTween = null;

		if (!animateChange || !gameObject.activeInHierarchy || Mathf.Approximately(currentFill, targetFill))
		{
			health.fillAmount = targetFill;
			return;
		}

		if (targetFill < currentFill)
		{
			healthTween = health.DOFillAmount(targetFill, damageTweenDuration).SetEase(damageTweenEase);
			return;
		}

		healthTween = health.DOFillAmount(targetFill, healTweenDuration).SetEase(healTweenEase);
	}

	public void SetHPImmediate(float hpNormalized)
	{
		SetHP(hpNormalized, false);
	}
}
