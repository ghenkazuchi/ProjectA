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

	[Header("Map View Controls")]
	[SerializeField] private float mapPanSpeed = 20f;
	[SerializeField] private float mouseDragSensitivity = 1f;
	
	private Vector3 lastMousePosition;

	void LateUpdate()
	{
		// Sync with PortalManager state
		if (PortalManager.Instance != null)
		{
			isMapView = PortalManager.Instance.IsMapViewActive;
		}

		if (!isMapView)
		{
			if (target == null) return;
			Vector3 desiredPosition = target.position + offset;
			Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
			transform.position = smoothedPosition;
		}
		else
		{
			// WASD Panning
			Vector3 panInput = Vector3.zero;
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) panInput.y = 1;
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) panInput.y = -1;
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) panInput.x = -1;
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) panInput.x = 1;

			transform.position += panInput.normalized * (mapPanSpeed * Time.deltaTime);

			// Mouse Drag Panning (Right Click or Middle Click)
			if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				lastMousePosition = Input.mousePosition;
			}
			else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
			{
				if (cam != null)
				{
					Vector3 delta = Input.mousePosition - lastMousePosition;
					float height = 2f * cam.orthographicSize;
					float width = height * cam.aspect;
					
					Vector3 worldDelta = new Vector3(
						(delta.x / Screen.width) * width,
						(delta.y / Screen.height) * height,
						0
					);
					
					transform.position -= worldDelta * mouseDragSensitivity;
				}
				lastMousePosition = Input.mousePosition;
			}
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