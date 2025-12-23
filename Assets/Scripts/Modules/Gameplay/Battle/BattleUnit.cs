using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public enum UnitType { PlayerUnit, MonsterUnit }
	
public class BattleUnit : BaseUnit
{
	[SerializeReference] public EntityBase character;
	[SerializeField] private GameObject highlightOverlay;
	[SerializeField] private HealthBar healthBar;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private UISpriteSheetPlayer hitVfx;
	public UnitState state;
	public UnitType type;
	public void SetUnitType(UnitType t) => type = t;

	public Vector2 GetOffSet(float dist = 50f)
	{
		if (type == UnitType.PlayerUnit)
			return new Vector2(dist, 0);
		else
			return new Vector2(-dist, 0);
	}
	public UISpriteSheetPlayer HitVfx => hitVfx;	
	[SerializeField] private BattleUnitAnimator Animator;
	public BattleUnitAnimator GetAnimator() => Animator;
	public override void SetUp()
	{
		unitImage.sprite = character.entityData.EntitySprite;
		level = character.level;
		highlightOverlay.SetActive(false);
		Debug.Log(character.GetCurrentHP());
		Debug.Log(character.MaxHp);
		healthBar.SetHP((float)character.GetCurrentHP() / character.MaxHp);
		state = UnitState.Alive;
		UpdateHP();

		var effectUI = GetComponentInChildren<BattleUnitActiveEffect>(true);
		if (effectUI != null && character != null) effectUI.Bind(character);
	}
	public void SetHighLight(bool isTargeted)
	{
		highlightOverlay.SetActive(isTargeted);
	}
	public void UpdateHP()
	{

		if (character.GetCurrentHP() <= 0)
		{
			state = UnitState.Dead;
			unitImage.sprite = deadSprite;
			SetHighLight(false);
		}
		else
		{
			state = UnitState.Alive;
			unitImage.sprite = character.entityData.EntitySprite;
		}
		healthBar.SetHP((float)character.GetCurrentHP() / character.MaxHp);
	}
	public bool IsAlive()
	{
		return state == UnitState.Alive;
	}
	public void PreviewDamage(int damage)
	{
		float futureHP = Mathf.Max(character.GetCurrentHP() - damage, 0);
		healthBar.SetHP(futureHP / character.MaxHp);
	}
	public void ClearPreview()
	{
		if (healthBar != null && character != null && character.MaxHp > 0)
			healthBar.SetHP((float)character.GetCurrentHP() / character.MaxHp);
	}
}
