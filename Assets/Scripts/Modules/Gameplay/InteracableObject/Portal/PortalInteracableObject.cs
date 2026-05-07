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

	private void Update()
	{
		// Fallback for OnMouseDown using manual Physics2D overlap check
		if (Input.GetMouseButtonDown(0))
		{
			if (PortalManager.Instance == null || !PortalManager.Instance.IsSelectingPortal) return;
			
			Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Collider2D col = GetComponent<Collider2D>();
			
			if (col != null && col.OverlapPoint(mouseWorldPos))
			{
				Debug.Log($"[Portal] Clicked on portal {portalDisplayName}. hasBeenDiscovered: {hasBeenDiscovered}, isSourcePortal: {PortalManager.Instance.SourcePortal == this}");

				if (!hasBeenDiscovered)
				{
					OverWorldUI overworldUI = FindObjectOfType<OverWorldUI>();
					if (overworldUI != null)
					{
						overworldUI.ShowToast("This portal hasn't been discovered yet!");
					}
					else
					{
						Debug.Log("[Portal] Cannot teleport: Destination portal is undiscovered!");
					}
					return;
				}
				if (PortalManager.Instance.SourcePortal == this)
				{
					Debug.Log("[Portal] Cannot teleport: You are already standing on this portal!");
					return;
				}

				if (UnityEngine.EventSystems.EventSystem.current != null && 
					UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
				{
					Debug.Log("[Portal] Click intercepted by UI! IsPointerOverGameObject is true.");
					// Commenting out the return so we can see if UI is blocking it
					// return; 
				}

				Debug.Log($"[Portal] Attempting to teleport to {portalDisplayName}...");
				PortalManager.Instance.SelectDestination(this);
			}
		}
	}
}
