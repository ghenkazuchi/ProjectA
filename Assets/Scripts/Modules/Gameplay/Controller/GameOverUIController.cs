using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HaKien;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
{
	[Header("Canvas")]
	[SerializeField] private CanvasGroup canvasGroup;

	[Header("UI Elements")]
	[SerializeField] private TextMeshProUGUI titleText;

	[Header("Buttons")]
	[SerializeField] private Button retryButton;
	[SerializeField] private Button mainMenuButt;
	[SerializeField] private Button exitGameButton;

	[Header("Animation")]
	[SerializeField] private RectTransform titleTransform;
	[SerializeField] private RectTransform buttonsContainer;
	[SerializeField] private float fadeDuration = 0.6f;
	[SerializeField] private float elementDelay = 0.2f;

	[Header("Scene Config")]
	[SerializeField] private string menuSceneName = "MenuScene";

	private Tween currentTween;

	private void Awake()
	{
		Hide();
		WireButtons();
	}

	private void OnDestroy()
	{
		currentTween?.Kill();
		UnwireButtons();
	}

	private void WireButtons()
	{
		if (retryButton != null)
			retryButton.onClick.AddListener(OnRetryClicked);
		if (mainMenuButt != null)
			mainMenuButt.onClick.AddListener(OnMainMenuClicked);
		if (exitGameButton != null)
			exitGameButton.onClick.AddListener(OnExitGameClicked);
	}

	private void UnwireButtons()
	{
		if (retryButton != null)
			retryButton.onClick.RemoveListener(OnRetryClicked);
		if (mainMenuButt != null)
			mainMenuButt.onClick.RemoveListener(OnMainMenuClicked);
		if (exitGameButton != null)
			exitGameButton.onClick.RemoveListener(OnExitGameClicked);
	}

	public void Show()
	{
		currentTween?.Kill();
		gameObject.SetActive(true);

		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		if (titleTransform != null)
			titleTransform.localScale = Vector3.one * 0.5f;
		if (buttonsContainer != null)
		{
			buttonsContainer.anchoredPosition = new Vector2(0f, -40f);
			var buttonsCg = buttonsContainer.GetComponent<CanvasGroup>();
			if (buttonsCg != null) buttonsCg.alpha = 0f;
		}

		Sequence seq = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);

		seq.Append(canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));

		// 2. Scale in the title
		if (titleTransform != null)
		{
			seq.Join(titleTransform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack));
		}

		// 3. Slide and fade in buttons
		if (buttonsContainer != null)
		{
			var buttonsCg = buttonsContainer.GetComponent<CanvasGroup>();
			seq.Append(buttonsContainer.DOAnchorPos(Vector2.zero, fadeDuration * 0.8f).SetEase(Ease.OutCubic).SetDelay(elementDelay));
			if (buttonsCg != null)
				seq.Join(buttonsCg.DOFade(1f, fadeDuration * 0.6f));
		}

		seq.OnComplete(() =>
		{
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		});

		currentTween = seq;
	}

	public void Hide()
	{
		currentTween?.Kill();
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
	}

	private void OnRetryClicked()
	{
		Debug.Log("[GameOver] Retry clicked — reloading InGame scene.");
		currentTween?.Kill();
		CleanupPersistentObjects();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void OnMainMenuClicked()
	{
		Debug.Log("[GameOver] Main Menu clicked — loading menu scene.");
		currentTween?.Kill();
		CleanupPersistentObjects();
		SceneManager.LoadScene(menuSceneName);
	}

	private void OnExitGameClicked()
	{
		Debug.Log("[GameOver] Exit Game clicked — quitting application.");
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	/// <summary>
	/// Destroys all DontDestroyOnLoad singleton objects so the next scene
	/// starts from a clean slate without stale references.
	/// </summary>
	public static void CleanupPersistentObjects()
	{
		// Destroy the Player object (PlayerParty uses its own DontDestroyOnLoad)
		if (PlayerParty.Instance != null)
		{
			Object.Destroy(PlayerParty.Instance.gameObject);
		}

		// Destroy all Singleton-based managers.
		// Each one lives on its own DontDestroyOnLoad GameObject with stale serialized refs.
		TryDestroySingleton(GameController.Instance);
		TryDestroySingleton(IngameUIManager.Instance);
		TryDestroySingleton(BattleSystem.Instance);
		TryDestroySingleton(AudioManager.Instance);
		TryDestroySingleton(DataManager.Instance);
		TryDestroySingleton(GameplayManager.Instance);
		TryDestroySingleton(PopupController.Instance);
		TryDestroySingleton(ChestOpenUIController.Instance);
		TryDestroySingleton(SkillLearnManager.Instance);
		TryDestroySingleton(PortalManager.Instance);
		TryDestroySingleton(PartyMemberInfoController.Instance);
		TryDestroySingleton(PartyMenuController.Instance);
		TryDestroySingleton(TimelineManager.Instance);
		TryDestroySingleton(RunProgressionManager.Instance);
		TryDestroySingleton(AchievementScreenController.Instance);
		TryDestroySingleton(GamePoolManager.Instance);
		TryDestroySingleton(MessageManager.Instance);
	}

	private static void TryDestroySingleton(Component instance)
	{
		if (instance != null)
		{
			Object.Destroy(instance.gameObject);
		}
	}
}
