using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class BattleUnitAnimator : MonoBehaviour
{
	[SerializeField] private RectTransform rect; 
	private Vector2 originalPos;
	[SerializeField] private Image unitSprite; 
	private Color originalColor;
	[SerializeField] private BattleUnit unit;
	void Awake()
	{
		originalPos = rect.anchoredPosition;
		originalColor = unitSprite.color;
	}
	public void PlayAttackAnimation(Vector2 direction)
	{
		Sequence sequence = DOTween.Sequence();
		
		// 1. ANTICIPATION: Pull back slightly and squish down
		sequence.Append(rect.DOAnchorPos(originalPos - (direction * 0.2f), 0.2f).SetEase(Ease.OutSine));
		sequence.Join(rect.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.2f));

		// 2. THE STRIKE: Snap forward super fast, stretch out
		sequence.Append(rect.DOAnchorPos(originalPos + direction, 0.1f).SetEase(Ease.OutBack, 2f));
		sequence.Join(rect.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.1f));

		// 3. RECOVERY: Settle back
		sequence.Append(rect.DOAnchorPos(originalPos, 0.3f).SetEase(Ease.InOutQuad));
		sequence.Join(rect.DOScale(Vector3.one, 0.3f));
	}
	public IEnumerator PlayHitAnimation(bool playDefaultVfx = true, Sprite[] overrideFrames = null,float overideFps = 0)
	{
		if(playDefaultVfx && unit.HitVfx != null)
		{
			if(overrideFrames != null && overrideFrames.Length > 0)
				unit.HitVfx.SetVFX(overrideFrames, overideFps > 0 ? overideFps : (float?)null);
			yield return unit.HitVfx.PlayOnce(0.5f);
		}
		
		Sequence sequence = DOTween.Sequence();
		
		// FLINCH: Get pushed back slightly
		sequence.Append(rect.DOAnchorPos(originalPos + new Vector2(-15f, 0f), 0.1f).SetEase(Ease.OutBounce));
		
		// SHAKE & FLASH
		sequence.Join(rect.DOShakeAnchorPos(0.4f, strength: 15f, vibrato: 20, randomness: 90, snapping: false, fadeOut: true));
		sequence.Join(unitSprite.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo));
		
		// SLIDE BACK
		sequence.Append(rect.DOAnchorPos(originalPos, 0.2f).SetEase(Ease.InOutQuad));
		
		yield return sequence.WaitForCompletion();
		unitSprite.color = originalColor;
	}
	public void PlayBuffPulse()
	{
		Sequence sequence = DOTween.Sequence();
		
		// Jump up, swell up, and flash positive color
		sequence.Append(rect.DOAnchorPos(originalPos + new Vector2(0f, 15f), 0.2f).SetEase(Ease.OutQuad));
		sequence.Join(rect.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5, 0.5f));
		sequence.Join(unitSprite.DOColor(new Color(0.8f, 1f, 0.8f), 0.2f).SetLoops(2, LoopType.Yoyo));
		
		// Float down
		sequence.Append(rect.DOAnchorPos(originalPos, 0.3f).SetEase(Ease.InQuad));
		
		sequence.OnComplete(() => unitSprite.color = originalColor);
	}
	public IEnumerator PlayParryAnimation(Sprite[] parryFrames)
	{
		unit.HitVfx.SetVFX(parryFrames);
		yield return unit.HitVfx.PlayOnce(0.5f);
	}
	public IEnumerator PlayHealingAnimation(Sprite[] healingFrames)
	{
		unit.HitVfx.SetVFX(healingFrames);
		yield return unit.HitVfx.PlayOnce(0.5f);
	}
}
