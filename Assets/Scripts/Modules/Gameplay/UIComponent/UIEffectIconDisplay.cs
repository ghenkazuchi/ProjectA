using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectIconDisplay : MonoBehaviour
{
	[SerializeField] private Image effectIcon;
	[SerializeField] private EffectBase currentEffect;
	public EffectBase CurrentEffect => currentEffect;

	public void Bind(EffectBase effect)
	{
		currentEffect = effect;
		if (effectIcon != null) effectIcon.sprite = effect.EffectIcon;
		RefreshNumbers();
	}

	public void RefreshNumbers()
	{
		if (currentEffect == null) return;
	}

	public void Clear()
	{
		currentEffect = null;
		if (effectIcon != null) effectIcon.sprite = null;
	}
}