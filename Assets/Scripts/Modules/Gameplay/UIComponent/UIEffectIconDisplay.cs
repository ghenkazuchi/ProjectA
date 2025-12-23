using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectIconDisplay : MonoBehaviour
{
	[SerializeField] private Image effectIcon;
	//[SerializeField] private TextMeshProUGUI durationText;
	//[SerializeField] private TextMeshProUGUI stackText; 
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
		//if (durationText != null)
		//	durationText.text = currentEffect.HasDuration ? currentEffect.CurrentDuration.ToString() : "∞";
		//if (stackText != null)
		//	stackText.text = (currentEffect.Stackable && currentEffect.CurrentStack > 1) ? $"x{currentEffect.CurrentStack}" : "";
	}

	public void Clear()
	{
		currentEffect = null;
		if (effectIcon != null) effectIcon.sprite = null;
		//if (durationText != null) durationText.text = "";
		//if (stackText != null) stackText.text = "";
	}
}