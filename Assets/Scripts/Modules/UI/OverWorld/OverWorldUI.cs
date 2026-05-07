using UnityEngine;
using TMPro;

public class OverWorldUI : MonoBehaviour
{
	[Header("Day/Night - Used for progression bar")]
	[SerializeField] private DayNightCycleController dayNightController;
	[SerializeField] private OverworldGameProgressUI gameProgressUI;
	[SerializeField] private CanvasGroup overWorldCanvasgroup;

	[Header("Currency Display")]
	[SerializeField] private TextMeshProUGUI goldText;
	[SerializeField] private TextMeshProUGUI soulDuskText;
	[Header("Toasts")]
	[SerializeField] private ToastUI toastUI;

	private void OnEnable()
	{
		if (DataManager.Instance != null && DataManager.Instance.Currency != null)
		{
			DataManager.Instance.Currency.Wallet.OnCurrencyChanged += HandleCurrencyChanged;
		}
	}

	private void OnDisable()
	{
		if (DataManager.Instance != null && DataManager.Instance.Currency != null)
		{
			DataManager.Instance.Currency.Wallet.OnCurrencyChanged -= HandleCurrencyChanged;
		}
	}

	private void HandleCurrencyChanged(CurrencyType type, int amount)
	{
		if (type == CurrencyType.Gold && goldText != null)
		{
			goldText.text = amount.ToString();
		}
		else if (type == CurrencyType.SoulDusk && soulDuskText != null)
		{
			soulDuskText.text = amount.ToString();
		}
	}

	private void Start()
	{
		if (dayNightController == null)
			dayNightController = FindAnyObjectByType<DayNightCycleController>();

		if (gameProgressUI != null && dayNightController != null)
			gameProgressUI.Bind(dayNightController);

		if (DataManager.Instance != null && DataManager.Instance.Currency != null)
		{
			if (goldText != null)
			{
				goldText.text = DataManager.Instance.Currency.Wallet.Get(CurrencyType.Gold).ToString();
			}
			if (soulDuskText != null)
			{
				soulDuskText.text = DataManager.Instance.Currency.Wallet.Get(CurrencyType.SoulDusk).ToString();
			}

			// Also ensure event is hooked up if Start happens after OnEnable
			DataManager.Instance.Currency.Wallet.OnCurrencyChanged -= HandleCurrencyChanged;
			DataManager.Instance.Currency.Wallet.OnCurrencyChanged += HandleCurrencyChanged;
		}
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

	public void ShowToast(string message)
	{
		Debug.Log($"[OverWorldUI] ShowToast called with message: {message}");
		if (toastUI != null)
		{
			toastUI.ShowToast(message);
		}
		else
		{
			Debug.LogError("[OverWorldUI] toastUI reference is missing! Please assign it in the inspector.");
		}
	}
}
