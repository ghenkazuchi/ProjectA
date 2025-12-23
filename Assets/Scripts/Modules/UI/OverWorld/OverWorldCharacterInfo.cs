using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverWorldCharacterInfo : MonoBehaviour
{
	[SerializeField] private EntityBase currentCharacter;
	[SerializeField] TextMeshProUGUI characterNameText;
	[SerializeField] TextMeshProUGUI characterLevelText;
	[SerializeField] TextMeshProUGUI characterClassText;

	[SerializeField] CharacterEquipmentSlotUI[] itemSlots;

}
