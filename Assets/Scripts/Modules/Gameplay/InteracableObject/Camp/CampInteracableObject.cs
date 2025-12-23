using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampInteracableObject : Interacable, ICampInteracable
{
	public float hpHeal = 0.4f; 
	public float mpHeal = 0.3f;  
	public float spHeal = 0.3f;  

	public PlayerParty playerParty;

	public bool destroyAfterRest = true;

	[Header("Visual")]

	public Color dayColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
	public Color nightColor = Color.white;

	[Header("Light2D")]
	public Light2D campLight;
	//light set up
	public float nightOuterRadius = 4.0f;
	public float nightInnerRadius = 1.2f;
	public float nightIntensity = 1.05f;
	[Header("Referendce")]
	DayNightCycleController daynightController;
	SpriteRenderer spriteRenderer;
	Collider2D collider;

	public void GetPlayerParty()
	{
		playerParty = GameController.Instance.GetPlayerParty();
	}

	public void Awake()
	{
		GetPlayerParty();
		daynightController = FindAnyObjectByType<DayNightCycleController>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
		collider = GetComponent<Collider2D>();
		daynightController.OnNightStateChanged += OnNightStateChanged;
		ApplyNightState(daynightController.isNight);
	}
	void OnDestroy()
	{
		if (daynightController != null) daynightController.OnNightStateChanged -= OnNightStateChanged;
	}

	protected override bool CanInteract(GameObject interactor)
	{
		bool freeRoam = GameController.Instance.currentState == GameState.FreeRoam;
		return freeRoam && daynightController.isNight;
	}
	public override void TriggerInteraction()
	{
		if (daynightController != null && !daynightController.isNight) return;

		MessageManager.Instance.SendMessage(new Message(
			MessageType.OnFireCampPopupOpen,
			new object[] {
				(Action<bool>)((yes) =>
				{
					if (yes)
				{
					HealParty();
					daynightController.AdvanceToNextMorning();
					if (destroyAfterRest) Destroy(gameObject);
				}
                MessageManager.Instance.SendMessage(new Message(MessageType.OnInteractEnd));
				})
			}
		));
	}

	private void OnNightStateChanged(bool isNight) => ApplyNightState(isNight);

	private void ApplyNightState(bool isNight)
	{
		if (collider != null) collider.enabled = isNight;
		if (spriteRenderer != null)
		{
			var tint = isNight ? nightColor : dayColor;
			spriteRenderer.color = tint;
		}

		if (campLight != null)
		{
			if (isNight)
			{
				campLight.enabled = true;
				campLight.intensity = nightIntensity;
				campLight.pointLightOuterRadius = nightOuterRadius;
				campLight.pointLightInnerRadius = nightInnerRadius;
			}
			else
			{
				campLight.intensity = 0f;
				campLight.pointLightOuterRadius = 0f;
				campLight.pointLightInnerRadius = 0f;
				campLight.enabled = false;
			}
		}
	}

	public void HealParty()
	{
		Debug.Log("Party healed at camp");
		foreach (var member in playerParty.partySlots)
		{
			if (member.entity is PlayerCharacter playerCharacter)
			{
				playerCharacter.Heal(Mathf.CeilToInt(member.entity.MaxHp * hpHeal));
				playerCharacter.RestoreMP(Mathf.CeilToInt(member.entity.MaxMP * mpHeal));
				playerCharacter.RestoreSP(Mathf.CeilToInt(member.entity.MaxSP * spHeal));
			}
		}
	}

}
