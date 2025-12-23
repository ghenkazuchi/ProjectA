using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberExpInfo : MonoBehaviour
{

	[SerializeReference] public PlayerCharacter characterData;
	[SerializeField] TextMeshProUGUI characterName;
	[SerializeField] TextMeshProUGUI level;
	[SerializeField] Image expBar;
	[SerializeField] private Color normalLevelColor;
	[SerializeField] private Color levelUpColor;
	[SerializeField] private float expFillDuration = 0.5f;

	private GridPosition gridPosition;
	private ExpDistributionController expDistributionController;

	public void AnimateExpGain(int expGained)
	{
		var playerCharacter = characterData as PlayerCharacter;
		if (playerCharacter == null) return;

		StartCoroutine(AnimateExpGainCoroutine(expGained));
	}

	private IEnumerator AnimateExpGainCoroutine(int expGained)
	{
		var playerCharacter = characterData as PlayerCharacter;
		if (playerCharacter == null) yield break;

		int beforeExp = playerCharacter.CurrentExp;
		int beforeLevel = playerCharacter.Level;
		int beforeExpNeeded = playerCharacter.GetExpNeededForNextLevel();
		float beforeFill = (float)beforeExp / beforeExpNeeded;

		playerCharacter.AddExp(expGained); 

		int afterExp = playerCharacter.CurrentExp;
		int afterLevel = playerCharacter.Level;
		int afterExpNeeded = playerCharacter.GetExpNeededForNextLevel();
		float afterFill = (float)afterExp / afterExpNeeded;

		if (afterLevel > beforeLevel)
		{
			yield return expBar.DOFillAmount(1f, expFillDuration).WaitForCompletion();
			expBar.fillAmount = 0f;
			level.color = levelUpColor;
			yield return expBar.DOFillAmount(afterFill, expFillDuration).WaitForCompletion();
		}
		else
		{
			yield return expBar.DOFillAmount(afterFill, expFillDuration).WaitForCompletion();
		}

		UpdateDisplayAfterExpGain();
	}

	public void SetUp(EntityBase entity, GridPosition position, ExpDistributionController controller)
	{
		this.characterData = entity as PlayerCharacter;
		this.gridPosition = position;
		this.expDistributionController = controller;

		Debug.Log($"PartyMemberExpInfo SetUp: {gameObject.name} at position ({position.x},{position.y})");

		level.color = normalLevelColor;
		UpdateDisplay();
	}


	private void UpdateDisplayAfterExpGain()
	{
		if (characterData != null)
		{
			characterName.text = characterData.entityData.EntityName;
			level.text = $"Level {characterData.Level}";
		}
	}


	public EntityBase GetCharacterData()
	{
		return characterData;
	}

	private void UpdateDisplay()
	{
		if (characterData != null)
		{
			characterName.text = characterData.entityData.EntityName;
			level.text = $"Level {characterData.Level}";
			float expRatio = (float)characterData.CurrentExp / characterData.GetExpNeededForNextLevel();
			expBar.fillAmount = expRatio;
		}
		else
		{
			characterName.text = "Empty Slot";
			level.text = "";
			expBar.fillAmount = 0f;
		}
	}
}