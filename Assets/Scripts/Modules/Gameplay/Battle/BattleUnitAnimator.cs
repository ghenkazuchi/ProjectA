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
		sequence.Append(rect.DOAnchorPos(originalPos + direction, 0.25f).SetEase(Ease.OutQuad));
		sequence.Append(rect.DOAnchorPos(originalPos, 0.25f).SetEase(Ease.InQuad));

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
		sequence.Append(rect.DOShakeAnchorPos(0.5f, strength: 10f, vibrato: 15, randomness: 45, snapping: false, fadeOut: true));
		sequence.Join(unitSprite.DOColor(Color.black, 0.35f).SetLoops(2, LoopType.Yoyo));
		yield return sequence.WaitForCompletion();
		unitSprite.color = originalColor	;

	}
	public void PlayBuffPulse()
	{
		rect.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5, 0.5f);
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
