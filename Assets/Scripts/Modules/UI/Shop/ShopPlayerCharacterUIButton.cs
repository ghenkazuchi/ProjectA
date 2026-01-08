using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPlayerCharacterUIButton : MonoBehaviour
{
	[SerializeField] private Image characterPortrait;
	private PlayerCharacter playerCharacter;

	public void Setup(PlayerCharacter character)
	{
		characterPortrait.sprite = character.entityData.EntitySprite;
		playerCharacter = character;
	}
}
