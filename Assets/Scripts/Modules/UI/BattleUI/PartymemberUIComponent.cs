using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartymemberUIComponent : MonoBehaviour
{

	[SerializeField] private BattleUnit battleUnit;
	[SerializeField] private TextMeshProUGUI partymemberNameText;
	[SerializeField] private Button button;
	private int index;
	private Action<int, BattleUnit> onClick;
	private bool selected;

	public void Bind(int index, Action<int, BattleUnit> onClick)
	{
		this.index = index;
		this.onClick = onClick;

		Debug.Log($"[PMUI] Bind slot={index} button={(button ? button.name : "NULL")} battleUnit={(battleUnit ? battleUnit.name : "NULL")}", this);

		if (button != null)
		{
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() =>
			{
				Debug.Log($"[PMUI] CLICK slot={this.index} battleUnit={(battleUnit ? battleUnit.name : "NULL")} hasChar={(battleUnit && battleUnit.character != null)}", this);
				this.onClick?.Invoke(this.index, this.battleUnit);
			});
		}
		else
		{
			Debug.LogError($"[PMUI] slot={index} has NO button reference!", this);
		}

		Refresh();
		SetSelected(false);
	}


	public void Refresh()
	{
		if(battleUnit == null || battleUnit.character == null)
		{
			partymemberNameText.text = "Empty";
			return;
		}
		partymemberNameText.text = battleUnit.character.entityData.EntityName;
	}

	public void SetSelected(bool value)
	{
		selected = value;
	}
}
