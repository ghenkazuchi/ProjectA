using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
	[SerializeField] Image elementalIcon;
	[SerializeField] TextMeshProUGUI skillNameText;
	[SerializeField] ElementalMapping elementalMapping;
	[SerializeField] Color normalColor;
	[SerializeField] Color highlightedColor;
	[SerializeField] Color disableColor;

	public bool isDisabled;
	public void SetUp(string skillName, Element? element)
	{
		if (element.HasValue)
		{
			Sprite icon = elementalMapping.GetIcon(element.Value);
			elementalIcon.sprite = icon;
			elementalIcon.gameObject.SetActive(true);
		}
		else
		{
			elementalIcon.sprite = null;
			elementalIcon.gameObject.SetActive(false);
		}
		skillNameText.text = skillName;
	}
	public void HighlightSkill(bool highlighted)
	{
		if (isDisabled)
		{
			skillNameText.color = disableColor;
			return;
		}
		skillNameText.color = highlighted ? highlightedColor : normalColor;
	}

	public void SetUnuseable(bool isAllowed)
	{
		isDisabled = !isAllowed;

		skillNameText.color = isDisabled ? disableColor : normalColor;

		var c = elementalIcon.color;
		c.a = isDisabled ? 0.35f : 1f; 
		elementalIcon.color = c;
	}
}
