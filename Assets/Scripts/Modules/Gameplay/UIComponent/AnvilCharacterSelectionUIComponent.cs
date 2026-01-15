using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnvilCharacterSelectionUIComponent : MonoBehaviour
{
	[SerializeField] private Image characterSprite;
	[SerializeField] private Button selectButton;
	private PlayerCharacter pc;
	private Action<PlayerCharacter> onSelectCharacter;

	public void SetUp(PlayerCharacter character, Action<PlayerCharacter> onSelectCharacter)
	{
		this.pc = character;
		if(pc != null)
		{
			Debug.Log(pc.entityData.EntityName);
		}
		else
		{
			Debug.Log("pc is null");
		}
		this.onSelectCharacter = onSelectCharacter;
		characterSprite.sprite = pc.entityData.EntitySprite;
		selectButton.onClick.RemoveAllListeners();
		selectButton.onClick.AddListener(() => onSelectCharacter?.Invoke(pc));
	}
}
