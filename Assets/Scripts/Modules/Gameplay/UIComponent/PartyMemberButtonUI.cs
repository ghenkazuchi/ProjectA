using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberButtonUI : MonoBehaviour
{
	[SerializeField] private Image characterPortrait;
	[SerializeField] private TextMeshProUGUI characterWeapon;
	[SerializeField] private TextMeshProUGUI characterItemSlotStatus;
	[SerializeField] Button button;
	public void SetupButton(PlayerCharacter character)
	{
		characterPortrait.sprite = character.entityData.EntityPortrait;
		characterWeapon.text = character.GetWeaponStatus();
		characterItemSlotStatus.text = character.GetItemSlotStatus();
	}
}
