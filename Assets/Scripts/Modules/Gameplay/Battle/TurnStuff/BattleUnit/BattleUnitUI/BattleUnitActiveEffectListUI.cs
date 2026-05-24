using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitActiveEffectListUI : MonoBehaviour
{
	[SerializeField] private BattleUnit selectedUnit;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Transform listSpawnPosition;
	[SerializeField] private BattleUnitActiveEffectUIComponent effectPrefab;
	[SerializeField] private BattleUnitCurentStatUI currentStatUI;
	[SerializeField] private ScrollRect effectListScrollRect;
	[SerializeField] private Button closeButton;

	private EntityBase boundEntity;
	private readonly List<BattleUnitActiveEffectUIComponent> spawnedEffects = new List<BattleUnitActiveEffectUIComponent>();

	private void Awake()
	{
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Hide);
		}
		Hide();
	}

	/// <summary>
	/// Show the panel for the given BattleUnit. Populates stats and effect list.
	/// </summary>
	public void Show(BattleUnit battleUnit)
	{
		if (battleUnit == null || battleUnit.character == null) return;

		// If clicking the same unit that's already shown, toggle off
		if (selectedUnit == battleUnit && IsVisible())
		{
			Hide();
			return;
		}

		Unbind();

		selectedUnit = battleUnit;
		boundEntity = battleUnit.character;

		// Bind stat display
		if (currentStatUI != null)
			currentStatUI.Bind(battleUnit);

		// Subscribe to entity events for live updates
		boundEntity.OnEffectAdded += OnEffectAdded;
		boundEntity.OnEffectRemoved += OnEffectRemoved;
		boundEntity.OnEffectChanged += OnEffectChanged;

		RebuildEffectList();
		SetVisible(true);
	}

	/// <summary>
	/// Hide the panel and unbind from current entity.
	/// </summary>
	public void Hide()
	{
		Unbind();
		SetVisible(false);

		var tut = TutorialSequenceRunner.Instance;
		if (tut != null && tut.IsWaitingForEffectListClose)
		{
			tut.OnEffectListClosed();
		}
	}

	private void OnDestroy()
	{
		Unbind();
	}

	private void Unbind()
	{
		if (boundEntity != null)
		{
			boundEntity.OnEffectAdded -= OnEffectAdded;
			boundEntity.OnEffectRemoved -= OnEffectRemoved;
			boundEntity.OnEffectChanged -= OnEffectChanged;
			boundEntity = null;
		}

		selectedUnit = null;
		ClearSpawnedEffects();

		if (currentStatUI != null)
			currentStatUI.Clear();
	}

	private void RebuildEffectList()
	{
		ClearSpawnedEffects();

		if (boundEntity == null) return;

		var allEffects = boundEntity.GetAllEffect();
		if (allEffects == null) return;

		Transform parent = listSpawnPosition != null ? listSpawnPosition : transform;

		foreach (var effect in allEffects)
		{
			if (effect == null) continue;
			// Skip passive equipment effects (same filter as BattleUnitActiveEffect)
			if (effect.SourceData != null && effect.SourceData.isPassiveEquipmentEffect) continue;

			SpawnEffectEntry(effect, parent);
		}
	}

	private void SpawnEffectEntry(EffectBase effect, Transform parent)
	{
		if (effectPrefab == null) return;

		var instance = Instantiate(effectPrefab, parent, false);
		instance.gameObject.SetActive(true);
		instance.Bind(effect);
		spawnedEffects.Add(instance);
	}

	private void ClearSpawnedEffects()
	{
		foreach (var entry in spawnedEffects)
		{
			if (entry != null)
				Destroy(entry.gameObject);
		}
		spawnedEffects.Clear();
	}

	// --- Event handlers for live updates ---

	private void OnEffectAdded(EntityBase entity, EffectBase effect)
	{
		if (effect.SourceData != null && effect.SourceData.isPassiveEquipmentEffect) return;

		Transform parent = listSpawnPosition != null ? listSpawnPosition : transform;
		SpawnEffectEntry(effect, parent);

		// Refresh stats since effects can modify HP/MP/SP
		if (currentStatUI != null)
			currentStatUI.RefreshStats();
	}

	private void OnEffectRemoved(EntityBase entity, EffectBase effect)
	{
		// Rebuild the entire list to stay in sync after removal
		RebuildEffectList();

		if (currentStatUI != null)
			currentStatUI.RefreshStats();
	}

	private void OnEffectChanged(EntityBase entity, EffectBase effect)
	{
		// Refresh all effect entries (stacks/duration may have changed)
		foreach (var entry in spawnedEffects)
		{
			if (entry != null)
				entry.Refresh();
		}

		if (currentStatUI != null)
			currentStatUI.RefreshStats();
	}

	// --- Visibility helpers ---

	private bool IsVisible()
	{
		if (canvasGroup != null)
			return canvasGroup.alpha > 0f;
		return gameObject.activeSelf;
	}

	private void SetVisible(bool visible)
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = visible ? 1f : 0f;
			canvasGroup.blocksRaycasts = visible;
			canvasGroup.interactable = visible;
		}
		else
		{
			gameObject.SetActive(visible);
		}
	}
}
