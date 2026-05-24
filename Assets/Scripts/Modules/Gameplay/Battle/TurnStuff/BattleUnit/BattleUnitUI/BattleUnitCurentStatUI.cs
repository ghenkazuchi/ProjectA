using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitCurentStatUI : MonoBehaviour
{
	public BattleUnit currentBattleUnit;
	[SerializeField] private Image currentBattleUnitPortrait;
	[SerializeField] private TextMeshProUGUI currentHpText;
	[SerializeField] private TextMeshProUGUI currentMpText;
	[SerializeField] private TextMeshProUGUI currentSpText;
	[SerializeField] private TextMeshProUGUI battleUnitNameText;

	public void Bind(BattleUnit battleUnit)
	{
		currentBattleUnit = battleUnit;
		if (battleUnit == null || battleUnit.character == null)
		{
			Clear();
			return;
		}

		var entity = battleUnit.character;

		if (currentBattleUnitPortrait != null)
		{
			currentBattleUnitPortrait.sprite = entity.entityData.EntityPortrait != null
				? entity.entityData.EntityPortrait
				: entity.entityData.EntitySprite;
			currentBattleUnitPortrait.enabled = true;
		}

		RefreshStats();
	}

	public void RefreshStats()
	{
		if (currentBattleUnit == null || currentBattleUnit.character == null) return;

		var entity = currentBattleUnit.character;

		if (currentHpText != null)
			currentHpText.text = $"HP: {entity.GetCurrentHP()} / {entity.MaxHp}";

		if (currentMpText != null)
			currentMpText.text = $"MP: {entity.GetCurrentMP()} / {entity.MaxMP}";

		if (currentSpText != null)
			currentSpText.text = $"SP: {entity.GetCurrentSP()} / {entity.MaxSP}";
		if(battleUnitNameText != null)
			battleUnitNameText.text = entity.entityData.EntityName;
	}

	public void Clear()
	{
		currentBattleUnit = null;
		if (currentBattleUnitPortrait != null) currentBattleUnitPortrait.enabled = false;
		if (currentHpText != null) currentHpText.text = "";
		if (currentMpText != null) currentMpText.text = "";
		if (currentSpText != null) currentSpText.text = "";
	}
}
