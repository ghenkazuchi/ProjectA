using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public enum UnitType { PlayerUnit, MonsterUnit }
	
public class BattleUnit : BaseUnit, IPointerClickHandler
{
	[SerializeReference] public EntityBase character;
	[SerializeField] private GameObject highlightOverlay;
	[SerializeField] private HealthBar healthBar;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private UISpriteSheetPlayer hitVfx;
	[SerializeField] private BattleUnitStatusVfxPresenter statusVfxPresenter;

	// Shared UI panel — all BattleUnits reference the same instance
	private static BattleUnitActiveEffectListUI effectListUI;

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

	private void Awake()
	{
		EnsureStatusVfxPresenter();
	}

	public override void SetUp()
	{
		EnsureStatusVfxPresenter();
		if (character == null)
		{
			state = UnitState.Empty;
			highlightOverlay.SetActive(false);

			if (unitImage != null)
			{
				unitImage.enabled = false;
				Button btn = unitImage.GetComponent<Button>();
				if (btn != null) btn.onClick.RemoveAllListeners();
			}
			if (healthBar != null) healthBar.SetHPImmediate(0f);

			var effectUI = GetComponentInChildren<BattleUnitActiveEffect>(true);
			if (effectUI != null) effectUI.gameObject.SetActive(false);
			statusVfxPresenter?.Bind(null);

			return;
		}

		state = character.GetCurrentHP() > 0 ? UnitState.Alive : UnitState.Dead;

		if (unitImage != null)
		{
			unitImage.enabled = true;
			unitImage.sprite = state == UnitState.Dead ? deadSprite : character.entityData.EntitySprite;

			// Ensure the image can be clicked
			Button btn = unitImage.GetComponent<Button>();
			if (btn == null)
			{
				btn = unitImage.gameObject.AddComponent<Button>();
				btn.transition = Selectable.Transition.None;
			}
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(() => OnPointerClick(null));
		}

		level = character.level;
		highlightOverlay.SetActive(false);

		if (healthBar != null)
			healthBar.SetHPImmediate((float)character.GetCurrentHP() / character.MaxHp);

		var effectUI2 = GetComponentInChildren<BattleUnitActiveEffect>(true);
		if (effectUI2 != null)
		{
			effectUI2.gameObject.SetActive(true);
			effectUI2.Bind(character);
		}
		statusVfxPresenter?.Bind(character);
	}

	public void SetHighLight(bool isTargeted)
	{
		highlightOverlay.SetActive(isTargeted);
	}
	public void UpdateHP()
	{
		if (state == UnitState.Empty || character == null) return;

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

		if (healthBar != null)
			healthBar.SetHP((float)character.GetCurrentHP() / character.MaxHp);
	}
	public bool IsAlive()
	{
		return state == UnitState.Alive;
	}
	public void PreviewDamage(int damage)
	{
		if (state == UnitState.Empty || character == null) return;

		float futureHP = Mathf.Max(character.GetCurrentHP() - damage, 0);
		healthBar.SetHPImmediate(futureHP / character.MaxHp);
	}

	public void PreviewHealing(int healingAmount)
	{
		if (state == UnitState.Empty || character == null) return;
		float futureHP = character.GetCurrentHP() + healingAmount;
		futureHP = Mathf.Clamp(futureHP,0,character.MaxHp);
		healthBar.SetHPImmediate(futureHP / character.MaxHp);
	}
	public void ClearPreview()
	{
		if (healthBar != null && character != null && character.MaxHp > 0)
			healthBar.SetHPImmediate((float)character.GetCurrentHP() / character.MaxHp);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (state == UnitState.Empty || character == null) return;

		// Tutorial: if waiting for a unit click, route through the tutorial system
		var tut = TutorialSequenceRunner.Instance;
		if (tut != null && tut.IsWaitingForUnitClick)
		{
			tut.OnUnitClicked(this);
			return;
		}

		// Block all normal unit inspection during tutorial transitions
		if (tut != null && tut.IsInputBlocked) return;

		// Only allow inspecting during the player's decision-making phase
		if (BattleSystem.Instance != null)
		{
			var bs = BattleSystem.Instance.battleState;
			if (bs != BattleState.ActionSelection && bs != BattleState.SkillSelection)
				return;
		}

		if (effectListUI == null)
			effectListUI = FindFirstObjectByType<BattleUnitActiveEffectListUI>(FindObjectsInactive.Include);

		if (effectListUI != null)
			effectListUI.Show(this);
	}

	private void EnsureStatusVfxPresenter()
	{
		if (statusVfxPresenter == null)
		{
			statusVfxPresenter = GetComponent<BattleUnitStatusVfxPresenter>();
		}

		if (statusVfxPresenter == null)
		{
			statusVfxPresenter = gameObject.AddComponent<BattleUnitStatusVfxPresenter>();
		}

		statusVfxPresenter.Initialize(unitImage != null ? unitImage.transform : transform);
	}
}
