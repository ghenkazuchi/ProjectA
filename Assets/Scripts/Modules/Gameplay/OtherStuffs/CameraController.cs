using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform target;
	public float smoothSpeed = 0.125f;
	public Vector3 offset = new Vector3(0, 0, -10);

	[Header("Map View")]
	[SerializeField] private float mapViewOrthoSize = 25f;
	[SerializeField] private float zoomSpeed = 5f;

	private Camera cam;
	private float defaultOrthoSize;
	private bool isMapView = false;

	public bool IsMapView => isMapView;

	private void Start()
	{
		cam = GetComponent<Camera>();
		if (cam != null)
		{
			defaultOrthoSize = cam.orthographicSize;
		}
	}

	void LateUpdate()
	{
		// Sync with PortalManager state
		if (PortalManager.Instance != null)
		{
			isMapView = PortalManager.Instance.IsMapViewActive;
		}

		if (!isMapView)
		{
			Vector3 desiredPosition = target.position + offset;
			Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
			transform.position = smoothedPosition;
		}

		// Smoothly transition camera zoom
		if (cam != null)
		{
			float targetSize = isMapView ? mapViewOrthoSize : defaultOrthoSize;
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
		}
	}

	public void ToggleMapView()
	{
		PortalManager.Instance?.ToggleMapView();
	}
}