using TMPro;
using UnityEngine;

public class PortalHighlighter : MonoBehaviour
{
	[SerializeField] private GameObject highlightObject;
	[SerializeField] private float highlightScale = 0.25f;

	private Vector3 originalScale;
	private PortalInteracableObject portal;

	private void Start()
	{
		originalScale = transform.localScale;
		portal = GetComponent<PortalInteracableObject>();

		if (highlightObject != null)
		{
			if (highlightObject == gameObject)
			{
				Debug.LogWarning("[PortalHighlighter] Highlight Object is assigned to the Portal itself! Clearing it to prevent bugs.");
				highlightObject = null;
			}
			else
			{
				highlightObject.SetActive(false);
			}
		}
	}

	private void OnEnable()
	{
		if (PortalManager.Instance != null)
		{
			PortalManager.Instance.OnMapViewToggled += HandleMapViewToggled;
		}
	}

	private void OnDisable()
	{
		if (PortalManager.Instance != null)
		{
			PortalManager.Instance.OnMapViewToggled -= HandleMapViewToggled;
		}
	}

	private void HandleMapViewToggled(bool isMapView)
	{
		bool isDiscovered = portal != null && portal.HasBeenDiscovered;
		bool isSource = PortalManager.Instance != null &&
		                PortalManager.Instance.SourcePortal == portal;

		if (isMapView)
		{
			// Scale up so portal is visible from zoomed-out view
			transform.localScale = new Vector3(highlightScale, highlightScale, originalScale.z);

			if (highlightObject != null)
			{
				highlightObject.SetActive(isDiscovered);
			}
		}
		else
		{
			transform.localScale = originalScale;

			if (highlightObject != null)
			{
				highlightObject.SetActive(false);
			}
		}
	}
}
