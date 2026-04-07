using HaKien;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : Singleton<PortalManager>
{
	private readonly List<PortalInteracableObject> allPortals = new List<PortalInteracableObject>();
	private readonly HashSet<PortalInteracableObject> discoveredPortals = new HashSet<PortalInteracableObject>();

	private bool isMapViewActive = false;
	private bool isSelectingPortal = false;
	private PortalInteracableObject sourcePortal;

	public bool IsMapViewActive => isMapViewActive;
	public bool IsSelectingPortal => isSelectingPortal;
	public PortalInteracableObject SourcePortal => sourcePortal;

	public event Action<bool> OnMapViewToggled;

	public void RegisterPortal(PortalInteracableObject portal)
	{
		if (portal != null && !allPortals.Contains(portal))
		{
			allPortals.Add(portal);
		}
	}

	public void UnregisterPortal(PortalInteracableObject portal)
	{
		allPortals.Remove(portal);
		discoveredPortals.Remove(portal);
	}

	public void DiscoverPortal(PortalInteracableObject portal)
	{
		if (portal != null)
		{
			discoveredPortals.Add(portal);
		}
	}

	public bool IsDiscovered(PortalInteracableObject portal)
	{
		return portal != null && discoveredPortals.Contains(portal);
	}

	public List<PortalInteracableObject> GetDiscoveredPortals()
	{
		discoveredPortals.RemoveWhere(p => p == null);
		return new List<PortalInteracableObject>(discoveredPortals);
	}

	public List<PortalInteracableObject> GetAllPortals()
	{
		allPortals.RemoveAll(p => p == null);
		return new List<PortalInteracableObject>(allPortals);
	}

	/// <summary>
	/// Called when a player interacts with a portal.
	/// Opens map view in "portal selection" mode.
	/// </summary>
	public void EnterPortalSelection(PortalInteracableObject source)
	{
		sourcePortal = source;
		isSelectingPortal = true;

		// Open map view
		if (!isMapViewActive)
		{
			isMapViewActive = true;
			MessageManager.Instance.SendMessage(new Message(MessageType.OnMapViewOpen));
			OnMapViewToggled?.Invoke(true);
		}

		MessageManager.Instance.SendMessage(new Message(MessageType.OnPortalOpen));
	}

	/// <summary>
	/// Called when a player clicks on a destination portal during selection.
	/// </summary>
	public void SelectDestination(PortalInteracableObject destination)
	{
		if (!isSelectingPortal || destination == null || destination == sourcePortal) return;
		if (!IsDiscovered(destination)) return;

		PlayerMovement player = FindObjectOfType<PlayerMovement>();
		if (player != null)
		{
			player.TeleportTo(destination.transform.position, destination.GridPosition);
		}

		ExitPortalSelection();
	}

	/// <summary>
	/// Cancels portal selection and closes map view.
	/// </summary>
	public void CancelPortalSelection()
	{
		if (isSelectingPortal)
		{
			ExitPortalSelection();
		}
	}

	private void ExitPortalSelection()
	{
		isSelectingPortal = false;
		sourcePortal = null;

		// Close map view
		if (isMapViewActive)
		{
			isMapViewActive = false;
			OnMapViewToggled?.Invoke(false);
			MessageManager.Instance.SendMessage(new Message(MessageType.OnMapViewClose));
		}

		MessageManager.Instance.SendMessage(new Message(MessageType.OnPortalClose));
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInteractEnd));
	}

	/// <summary>
	/// Toggles map view for general browsing (non-portal-selection).
	/// </summary>
	public void ToggleMapView()
	{
		// If we're in portal selection mode, cancel it instead
		if (isSelectingPortal)
		{
			CancelPortalSelection();
			return;
		}

		isMapViewActive = !isMapViewActive;

		if (isMapViewActive)
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnMapViewOpen));
		}
		else
		{
			MessageManager.Instance.SendMessage(new Message(MessageType.OnMapViewClose));
		}

		OnMapViewToggled?.Invoke(isMapViewActive);
	}
}
