using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopPlayerCharacterUIButton : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI characterNameText;
	private PlayerCharacter playerCharacter;

	public void Setup(PlayerCharacter character)
	{
		characterNameText.text = character.entityData.EntityName;
		playerCharacter = character;
	}
}
