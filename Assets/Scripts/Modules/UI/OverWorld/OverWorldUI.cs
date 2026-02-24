using UnityEngine;

public class OverWorldUI : MonoBehaviour
{
	[Header("Day/Night - dùng cho thanh progression")]
	[SerializeField] private DayNightCycleController dayNightController;
	[SerializeField] private OverworldGameProgressUI gameProgressUI;
	[SerializeField] private CanvasGroup overWorldCanvasgroup;

	private void Start()
	{
		if (dayNightController == null)
			dayNightController = FindAnyObjectByType<DayNightCycleController>();

		if (gameProgressUI != null && dayNightController != null)
			gameProgressUI.Bind(dayNightController);
	}
	private void Awake()
	{
		Hide();
	}
	public void Hide()
	{
		overWorldCanvasgroup.alpha = 0f;
		overWorldCanvasgroup.interactable = false;
		overWorldCanvasgroup.blocksRaycasts = false;

	}
	public void Show()
	{
		overWorldCanvasgroup.alpha = 1f;
		overWorldCanvasgroup.interactable = true;
		overWorldCanvasgroup.blocksRaycasts = true;
	}
}
