using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI hpText;
	[SerializeField] TextMeshProUGUI mpText;
	[SerializeField] TextMeshProUGUI spText;
	public void SetData(EntityBase entity)
	{
		nameText.text = entity.entityData.EntityName;
		levelText.text = "Lvl: " + entity.Level;
		hpText.text = "HP: " + entity.GetCurrentHP().ToString();
		mpText.text = "MP: " + entity.GetCurrentMP().ToString();
		spText.text = "SP: " + entity.GetCurrentSP().ToString();
	}
	public void EnableBattleHud(bool enabled)
	{
		gameObject.SetActive(enabled);
	}
}
