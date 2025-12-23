using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberButtonUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI characterName;
	[SerializeField] private TextMeshProUGUI characterWeapon;
	[SerializeField] private TextMeshProUGUI characterItemSlotStatus;
	[SerializeField] Button button;
	public void SetupButton(PlayerCharacter character)
	{
		characterName.text = character.entityData.EntityName;
		characterWeapon.text = character.GetWeaponStatus();
		characterItemSlotStatus.text = character.GetItemSlotStatus();
	}
}
