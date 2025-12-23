using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleUnitActiveEffect : MonoBehaviour
{
	[Header("Single Panel (Grid/Horizontal)")]
	[SerializeField] private Transform effectPanel;
	[SerializeField] private UIEffectIconDisplay iconPrefab;
	[SerializeField] private int poolSize = 12;
	[SerializeField] private int maxIcons = 5;

	private readonly Queue<UIEffectIconDisplay> pool = new Queue<UIEffectIconDisplay>();
	private readonly Dictionary<int, UIEffectIconDisplay> id2Icon = new Dictionary<int, UIEffectIconDisplay>();
	private readonly List<int> displayOrder = new List<int>(); 
	private readonly Stack<int> evictedStack = new Stack<int>();

	private EntityBase entity;

	private void Awake()
	{
		for (int i = 0; i < poolSize; i++)
		{
			var inst = Instantiate(iconPrefab, transform);
			inst.gameObject.SetActive(false);
			pool.Enqueue(inst);
		}
	}

	public void Bind(EntityBase e)
	{
		if (entity == e)
		{
			RebuildInitial();
			return;
		}
		Unbind();
		entity = e;
		if (entity == null)
		{
			ClearAll();
			return;
		}
		entity.OnEffectAdded += OnEffectAdded;
		entity.OnEffectRemoved += OnEffectRemoved;
		entity.OnEffectChanged += OnEffectChanged;
		entity.OnEntityDead += OnUnitDeath;
		RebuildInitial();
	}

	private void OnDestroy() => Unbind();

	private void Unbind()
	{
		if (entity == null) return;
		entity.OnEffectAdded -= OnEffectAdded;
		entity.OnEffectRemoved -= OnEffectRemoved;
		entity.OnEffectChanged -= OnEffectChanged;
		entity.OnEntityDead -= OnUnitDeath;
		entity = null;
		ClearAll();
	}
	private void OnUnitDeath(EntityBase entity) => Unbind();
	private void OnDisable()
	{
		Unbind();
	}

	private void OnEffectAdded(EntityBase ent, EffectBase e)
	{
		int rid = e.RuntimeId;
		if (id2Icon.TryGetValue(rid, out var ui))
		{
			ui.Bind(e);
			return;
		}

		if (displayOrder.Count < maxIcons)
		{
			AddEffectToFront(e);
			ApplySiblingIndices();
			return;
		}
		int evictedRid = displayOrder[displayOrder.Count - 1];
		Evict(evictedRid, remember: true);

		AddEffectToFront(e);
		ApplySiblingIndices();
	}

	private void OnEffectChanged(EntityBase ent, EffectBase e)
	{
		if (id2Icon.TryGetValue(e.RuntimeId, out var ui))
		{
			ui.Bind(e);
		}
	}

	private void OnEffectRemoved(EntityBase ent, EffectBase e)
	{
		int rid = e.RuntimeId;

		if (id2Icon.TryGetValue(rid, out var ui))
		{
			id2Icon.Remove(rid);
			displayOrder.Remove(rid);
			ReturnUI(ui);
			ApplySiblingIndices();
		}
		TryFillVacancyFromEvictedOrOthers();
	}

	private void RebuildInitial()
	{
		ClearAll();
		var all = entity?.GetAllEffect();
		if (all == null || all.Count == 0) return;

		foreach (var e in all.Take(maxIcons))
		{
			var ui = GetUI();
			ui.transform.SetParent(effectPanel, false);
			ui.gameObject.SetActive(true);
			ui.Bind(e);
			id2Icon[e.RuntimeId] = ui;
			displayOrder.Add(e.RuntimeId);
		}
		displayOrder.Reverse();
		ApplySiblingIndices();
	}

	private void TryFillVacancyFromEvictedOrOthers()
	{
		if (entity == null) return;
		if (displayOrder.Count >= maxIcons) return;

		while (evictedStack.Count > 0 && displayOrder.Count < maxIcons)
		{
			int candRid = evictedStack.Pop();
			var eff = FindActiveEffectByRuntimeId(candRid);
			if (eff == null) continue; 
			if (id2Icon.ContainsKey(candRid)) continue; 
			AddEffectToFront(eff);
		}
		if (displayOrder.Count < maxIcons)
		{
			var all = entity.GetAllEffect();
			foreach (var eff in all)
			{
				if (displayOrder.Count >= maxIcons) break;
				if (id2Icon.ContainsKey(eff.RuntimeId)) continue;
				AddEffectToFront(eff);
			}
		}

		ApplySiblingIndices();
	}

	private EffectBase FindActiveEffectByRuntimeId(int rid)
	{
		var all = entity?.GetAllEffect();
		if (all == null) return null;
		for (int i = 0; i < all.Count; i++)
			if (all[i].RuntimeId == rid) return all[i];
		return null;
	}

	private void AddEffectToFront(EffectBase e)
	{
		var ui = GetUI();
		ui.transform.SetParent(effectPanel, false);
		ui.gameObject.SetActive(true);
		ui.Bind(e);

		id2Icon[e.RuntimeId] = ui;
		displayOrder.Insert(0, e.RuntimeId);
	}

	private void Evict(int rid, bool remember)
	{
		if (!id2Icon.TryGetValue(rid, out var ui)) return;
		id2Icon.Remove(rid);
		displayOrder.Remove(rid);
		if (remember) evictedStack.Push(rid);
		ReturnUI(ui);
	}

	private void ApplySiblingIndices()
	{
		for (int i = 0; i < displayOrder.Count; i++)
		{
			var rid = displayOrder[i];
			if (id2Icon.TryGetValue(rid, out var ui))
			{
				ui.transform.SetSiblingIndex(i);
			}
		}
	}

	private UIEffectIconDisplay GetUI() => pool.Count > 0 ? pool.Dequeue() : Instantiate(iconPrefab);

	private void ReturnUI(UIEffectIconDisplay ui)
	{
		ui.Clear();
		ui.gameObject.SetActive(false);
		ui.transform.SetParent(transform, false);
		pool.Enqueue(ui);
	}

	private void ClearAll()
	{
		foreach (var kv in id2Icon) ReturnUI(kv.Value);
		id2Icon.Clear();
		displayOrder.Clear();
		evictedStack.Clear();
	}
}	