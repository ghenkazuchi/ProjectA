using HaKien;
using UnityEngine;

public class PortalInteracableObject : Interacable
{
	private Vector2Int gridPosition;
	private string portalDisplayName;
	private bool hasBeenDiscovered = false;

	public Vector2Int GridPosition => gridPosition;
	public string PortalDisplayName => portalDisplayName;
	public bool HasBeenDiscovered => hasBeenDiscovered;

	private void OnEnable()
	{
		PortalManager.Instance?.RegisterPortal(this);
	}

	private void OnDisable()
	{
		PortalManager.Instance?.UnregisterPortal(this);
	}

	public void Initialize(Vector2Int gridPos, int portalIndex)
	{
		gridPosition = gridPos;
		portalDisplayName = $"Portal {portalIndex}";
	}

	public override void TriggerInteraction()
	{
		// First visit: discover this portal
		if (!hasBeenDiscovered)
		{
			hasBeenDiscovered = true;
			PortalManager.Instance?.DiscoverPortal(this);
			Debug.Log($"[Portal] Discovered {portalDisplayName} at {gridPosition}");
		}

		// Enter portal selection mode (opens map view)
		PortalManager.Instance?.EnterPortalSelection(this);
	}

	/// <summary>
	/// Called when this portal is clicked during map view portal selection.
	/// </summary>
	private void OnMouseDown()
	{
		if (PortalManager.Instance == null) return;
		if (!PortalManager.Instance.IsSelectingPortal) return;
		if (!hasBeenDiscovered) return;

		// Don't allow selecting the portal you're standing on
		if (PortalManager.Instance.SourcePortal == this) return;

		PortalManager.Instance.SelectDestination(this);
	}
}
