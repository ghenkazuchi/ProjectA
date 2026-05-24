using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitActiveEffectUIComponent : MonoBehaviour
{
	[SerializeField] private Image effectIcon;
	[SerializeField] private TextMeshProUGUI effectDescriptionText;
	[SerializeField] private TextMeshProUGUI remainingDurationText;
	[SerializeField] private TextMeshProUGUI effectName;

	private EffectBase boundEffect;

	public void Bind(EffectBase effect)
	{
		boundEffect = effect;
		if (effect == null)
		{
			Clear();
			return;
		}

		if (effectIcon != null)
			effectIcon.sprite = effect.EffectIcon;

		if (effectName != null)
			effectName.text = effect.Name;

		if (remainingDurationText != null)
		{
			if (!effect.HasDuration || effect.CurrentDuration > 10)
			{
				remainingDurationText.text = "∞";
			}
			else
			{
				remainingDurationText.text = $"{effect.CurrentDuration}";
			}
		}

		if (effectDescriptionText != null)
		{
			string desc = "";

			// Show effect type
			desc += effect.EffectType == EffectType.Buff ? "<color=#4CAF50>Buff</color>" : "<color=#F44336>Debuff</color>";

			// Show stacks if stackable
			if (effect.Stackable)
				desc += $" | Stack: {effect.CurrentStack}/{effect.MaxStack}";

			if (effect.SourceData != null && !string.IsNullOrEmpty(effect.SourceData.Description))
			{
				desc += $"\n{effect.SourceData.Description}";
			}

			effectDescriptionText.text = desc;
		}
	}

	public void Refresh()
	{
		if (boundEffect != null)
			Bind(boundEffect);
	}

	public void Clear()
	{
		boundEffect = null;
		if (effectIcon != null) effectIcon.sprite = null;
		if (effectName != null) effectName.text = "";
		if (remainingDurationText != null) remainingDurationText.text = "";
		if (effectDescriptionText != null) effectDescriptionText.text = "";
	}
}
