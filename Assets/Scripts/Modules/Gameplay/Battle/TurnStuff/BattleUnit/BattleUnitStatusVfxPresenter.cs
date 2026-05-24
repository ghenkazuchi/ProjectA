using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitStatusVfxPresenter : MonoBehaviour
{
	private struct QueuedStatusVfx
	{
		public Sprite[] frames;
		public float fps;
		public float fadeOut;
	}

	private readonly Queue<QueuedStatusVfx> pending = new Queue<QueuedStatusVfx>();
	private readonly HashSet<EffectBase> subscribedEffects = new HashSet<EffectBase>();

	private EntityBase entity;
	private UISpriteSheetPlayer player;
	private Coroutine playRoutine;

	public void Initialize(Transform parent)
	{
		if (player != null)
		{
			return;
		}

		Transform host = parent != null ? parent : transform;
		GameObject vfxObject = new GameObject("StatusVfx", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(UISpriteSheetPlayer));
		RectTransform rectTransform = vfxObject.GetComponent<RectTransform>();
		rectTransform.SetParent(host, false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.localScale = Vector3.one;

		Image image = vfxObject.GetComponent<Image>();
		image.raycastTarget = false;
		image.preserveAspect = true;

		player = vfxObject.GetComponent<UISpriteSheetPlayer>();
	}

	public void Bind(EntityBase targetEntity)
	{
		if (entity == targetEntity)
		{
			return;
		}

		Unbind(clearQueue: true);
		entity = targetEntity;
		if (entity == null)
		{
			return;
		}

		entity.OnEffectAdded += OnEffectAdded;
		entity.OnEffectRemoved += OnEffectRemoved;
		entity.OnEntityDead += OnEntityDead;

		List<EffectBase> activeEffects = entity.GetAllEffect();
		for (int i = 0; i < activeEffects.Count; i++)
		{
			SubscribeToEffect(activeEffects[i]);
		}
	}

	private void OnDestroy()
	{
		Unbind(clearQueue: true);
	}

	private void OnDisable()
	{
		ClearQueue();
	}

	private void OnEffectAdded(EntityBase _, EffectBase effect)
	{
		SubscribeToEffect(effect);
	}

	private void OnEffectRemoved(EntityBase _, EffectBase effect)
	{
		UnsubscribeFromEffect(effect);
	}

	private void OnEntityDead(EntityBase _)
	{
		// Let any already-requested fatal status burst finish playing,
		// but detach from the dead entity so no new requests arrive.
		Unbind(clearQueue: false);
	}

	private void SubscribeToEffect(EffectBase effect)
	{
		if (effect == null || !subscribedEffects.Add(effect))
		{
			return;
		}

		effect.OnVfxRequested += OnEffectVfxRequested;
	}

	private void UnsubscribeFromEffect(EffectBase effect)
	{
		if (effect == null || !subscribedEffects.Remove(effect))
		{
			return;
		}

		effect.OnVfxRequested -= OnEffectVfxRequested;
	}

	private void OnEffectVfxRequested(EffectBase effect, EffectVfxTrigger trigger)
	{
		if (player == null || effect == null || !effect.TryGetStatusVfx(trigger, out EffectVfxClipData clip))
		{
			return;
		}

		pending.Enqueue(new QueuedStatusVfx
		{
			frames = clip.frames,
			fps = clip.fps,
			fadeOut = clip.fadeOut
		});

		if (playRoutine == null)
		{
			playRoutine = StartCoroutine(PlayQueue());
		}
	}

	private IEnumerator PlayQueue()
	{
		while (pending.Count > 0)
		{
			QueuedStatusVfx queued = pending.Dequeue();
			player.SetVFX(queued.frames, queued.fps);
			yield return player.PlayOnce(queued.fadeOut);
		}

		playRoutine = null;
	}

	private void Unbind(bool clearQueue)
	{
		if (entity != null)
		{
			entity.OnEffectAdded -= OnEffectAdded;
			entity.OnEffectRemoved -= OnEffectRemoved;
			entity.OnEntityDead -= OnEntityDead;
			entity = null;
		}

		foreach (EffectBase effect in subscribedEffects)
		{
			if (effect != null)
			{
				effect.OnVfxRequested -= OnEffectVfxRequested;
			}
		}

		subscribedEffects.Clear();

		if (clearQueue)
		{
			ClearQueue();
		}
	}

	private void ClearQueue()
	{
		pending.Clear();

		if (playRoutine != null)
		{
			StopCoroutine(playRoutine);
			playRoutine = null;
		}

		player?.HideImmediate();
	}
}
