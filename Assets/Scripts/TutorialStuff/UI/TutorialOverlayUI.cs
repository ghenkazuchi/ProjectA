using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialOverlayUI : MonoBehaviour
{
	public static TutorialOverlayUI Instance { get; private set; }

	[Header("Overlay")]
	[SerializeField] private CanvasGroup overlayCanvasGroup;
	[SerializeField] private Image dimBackground;

	[Header("Highlight Frame")]
	[SerializeField] private RectTransform highlightFrame;
	[SerializeField] private Image highlightFrameImage;
	[SerializeField] private Sprite squareHighlightSprite;
	[SerializeField] private Sprite circleHighlightSprite;
	[SerializeField] private Sprite triangleHighlightSprite;

	[Header("Finger Icon")]
	[SerializeField] private RectTransform fingerIconRoot;
	[SerializeField] private Image fingerImage;

	[Header("Text Panel")]
	[SerializeField] private RectTransform textPanelRoot;
	[SerializeField] private TextMeshProUGUI guidanceText;
	[SerializeField] private GameObject tapToContinueIndicator;

	[Header("Highlightable References (assign in Inspector)")]
	[SerializeField] private RectTransform actionSelectorRect;
	[SerializeField] private RectTransform skillSelectorRect;
	[SerializeField] private RectTransform timelineRect;
	[SerializeField] private RectTransform enemyAreaRect;
	[SerializeField] private RectTransform allyAreaRect;
	[SerializeField] private RectTransform[] actionButtonRects; // Skill, Attack, Defend, Switch
	[SerializeField] private RectTransform[] skillButtonRects;  // 4 skill slots

	[Header("Battle Unit References")]
	[SerializeField] private BattleUnit[] playerBattleUnits;   // 6 slots
	[SerializeField] private BattleUnit[] monsterBattleUnits;  // 6 slots

	public BattleUnit[] PlayerBattleUnits => playerBattleUnits;
	public BattleUnit[] MonsterBattleUnits => monsterBattleUnits;

	[Header("Active Effect UI References")]
	[SerializeField] private RectTransform activeEffectContainerRect;
	[SerializeField] private RectTransform activeEffectExitButtonRect;

	[Header("Animation Settings")]
	[SerializeField] private float fingerBobSpeed = 3f;
	[SerializeField] private float fingerBobAmount = 15f;
	[SerializeField] private float fadeSpeed = 0.3f;

	private Coroutine fingerAnimCoroutine;
	private bool waitingForTap;
	private Vector3 fingerBasePos;

	public bool IsWaitingForTap => waitingForTap;

	public enum TutorialOverlayState
	{
		Hidden,
		FadingIn,
		WaitingForCorrectAction,
		WaitingForTap,
		FadingOut
	}

	private TutorialOverlayState overlayState = TutorialOverlayState.Hidden;
	public TutorialOverlayState CurrentState => overlayState;

	// Default state caching
	private Vector2 defaultTextPanelPos;
	private Vector2 defaultTextPanelSize;
	private float defaultFontSize;

	private void Awake()
	{
		Instance = this;
		
		if (textPanelRoot != null)
		{
			defaultTextPanelPos = textPanelRoot.anchoredPosition;
			defaultTextPanelSize = textPanelRoot.sizeDelta;
		}
		if (guidanceText != null)
		{
			defaultFontSize = guidanceText.fontSize;
		}

		gameObject.SetActive(false);
	}

	public IEnumerator ShowStep(TutorialStepData step)
	{
		gameObject.SetActive(true);
		overlayState = TutorialOverlayState.FadingIn;
		guidanceText.text = step.guidanceText;

		// Resolve the target RectTransform
		RectTransform target = ResolveTarget(step);

		if (target != null && step.pointerTarget != TutorialPointerTarget.None)
		{
			PositionHighlight(target, step);
			PositionFinger(target, step);
			highlightFrame.gameObject.SetActive(true);
			fingerIconRoot.gameObject.SetActive(true);
			StartFingerAnimation();
		}
		else
		{
			highlightFrame.gameObject.SetActive(false);
			fingerIconRoot.gameObject.SetActive(false);
		}

		// Apply Text Panel Overrides
		if (textPanelRoot != null)
		{
			if (step.hideTextPanel)
			{
				textPanelRoot.gameObject.SetActive(false);
			}
			else
			{
				textPanelRoot.gameObject.SetActive(true);
				textPanelRoot.anchoredPosition = step.customTextPosition != Vector2.zero ? step.customTextPosition : defaultTextPanelPos;
				textPanelRoot.sizeDelta = step.customTextPanelSize != Vector2.zero ? step.customTextPanelSize : defaultTextPanelSize;
			}
		}
		if (guidanceText != null && !step.hideTextPanel)
		{
			guidanceText.fontSize = step.customFontSize > 0 ? step.customFontSize : defaultFontSize;
		}

		tapToContinueIndicator.SetActive(step.interactionMode == TutorialInteractionMode.TapToContinue);

		yield return FadeIn();

		if (step.interactionMode == TutorialInteractionMode.TapToContinue)
		{
			overlayState = TutorialOverlayState.WaitingForTap;
			yield return WaitForTap();
			overlayState = TutorialOverlayState.FadingOut;
			yield return FadeOut();
			overlayState = TutorialOverlayState.Hidden;
		}
		else
		{
			overlayState = TutorialOverlayState.WaitingForCorrectAction;
		}
		// yield return null;
	}

	public IEnumerator HideOverlay()
	{
		overlayState = TutorialOverlayState.FadingOut;
		StopFingerAnimation();
		yield return FadeOut();
		gameObject.SetActive(false);
		overlayState = TutorialOverlayState.Hidden;
	}

	//Target Resolution

	private RectTransform ResolveTarget(TutorialStepData step)
	{
		// Specific unit targets
		if (step.pointerTarget == TutorialPointerTarget.SpecificUnit
			|| step.pointerTarget == TutorialPointerTarget.SpecificUnitHealthBar
			|| step.pointerTarget == TutorialPointerTarget.CurrentTurnUnitHealthBar)
		{
			BattleUnit unit = null;

			if (step.pointerTarget == TutorialPointerTarget.CurrentTurnUnitHealthBar)
			{
				// Find the current turn entity's BattleUnit
				var currentEntity = BattleSystem.Instance?.currentTurnEntity;
				if (currentEntity != null)
					unit = BattleSystem.Instance.FindBattleUnitForEntityPublic(currentEntity);
			}
			else if (step.targetSpecificUnit)
			{
				var units = step.targetUnitSide == UnitType.PlayerUnit ? playerBattleUnits : monsterBattleUnits;
				if (step.targetUnitSlotIndex >= 0 && step.targetUnitSlotIndex < units.Length)
					unit = units[step.targetUnitSlotIndex];
			}

			if (unit != null)
				return unit.GetComponent<RectTransform>();

			return null;
		}

		// Enum-based targets
		return step.pointerTarget switch
		{
			TutorialPointerTarget.ActionSelector => actionSelectorRect,
			TutorialPointerTarget.SkillSelector => skillSelectorRect,
			TutorialPointerTarget.Timeline => timelineRect,
			TutorialPointerTarget.EnemyUnits => enemyAreaRect,
			TutorialPointerTarget.AllyUnits => allyAreaRect,
			TutorialPointerTarget.SpecificAction_Skill => GetActionButton(0),
			TutorialPointerTarget.SpecificAction_Attack => GetActionButton(1),
			TutorialPointerTarget.SpecificAction_Defend => GetActionButton(2),
			TutorialPointerTarget.SpecificAction_Switch => GetActionButton(3),
			TutorialPointerTarget.SpecificSkill => GetSkillButton(step.requiredSkillIndex),
			TutorialPointerTarget.ActiveEffectContainer => activeEffectContainerRect,
			TutorialPointerTarget.ActiveEffectExitButton => activeEffectExitButtonRect,
			_ => null
		};
	}

	private RectTransform GetActionButton(int index)
	{
		if (actionButtonRects == null || index < 0 || index >= actionButtonRects.Length) return null;
		return actionButtonRects[index];
	}

	private RectTransform GetSkillButton(int index)
	{
		if (skillButtonRects == null || index < 0 || index >= skillButtonRects.Length) return null;
		return skillButtonRects[index];
	}

	//Positioning

	private Vector3 GetTrueCenter(RectTransform rect)
	{
		Vector3[] corners = new Vector3[4];
		rect.GetWorldCorners(corners);
		return (corners[0] + corners[2]) / 2f;
	}

	private void PositionHighlight(RectTransform target, TutorialStepData step)
	{
		if (target == null) return;

		// Swap the sprite
		if (highlightFrameImage != null)
		{
			highlightFrameImage.sprite = step.frameShape switch
			{
				HighlightShape.Circle => circleHighlightSprite,
				HighlightShape.Triangle => triangleHighlightSprite,
				_ => squareHighlightSprite
			};
		}

		//Size and position
		Vector3 trueCenter = GetTrueCenter(target);
		highlightFrame.position = trueCenter + (Vector3)step.customFrameOffset;
		if (step.customFrameSize != Vector2.zero)
		{
			highlightFrame.sizeDelta = step.customFrameSize;
		}
		else
		{
			highlightFrame.sizeDelta = target.rect.size + step.highlightPadding;
		}
	}

	private void PositionFinger(RectTransform target, TutorialStepData step)
	{
		if (target == null) return;

		// Size override
		if (step.customFingerSize != Vector2.zero)
		{
			fingerIconRoot.sizeDelta = step.customFingerSize;
		}

		Vector3 targetPos = GetTrueCenter(target);
		float offsetDistance = step.pointerOffset;

		// Rotate finger and position it based on direction
		switch (step.pointerDirection)
		{
			case PointerDirection.FromBelow:
				fingerIconRoot.position = targetPos + new Vector3(0, -offsetDistance, 0);
				fingerIconRoot.localEulerAngles = Vector3.zero; // Pointing up
				break;
			case PointerDirection.FromAbove:
				fingerIconRoot.position = targetPos + new Vector3(0, offsetDistance, 0);
				fingerIconRoot.localEulerAngles = new Vector3(0, 0, 180f); // Pointing down
				break;
			case PointerDirection.FromLeft:
				fingerIconRoot.position = targetPos + new Vector3(-offsetDistance, 0, 0);
				fingerIconRoot.localEulerAngles = new Vector3(0, 0, -90f); // Pointing right
				break;
			case PointerDirection.FromRight:
				fingerIconRoot.position = targetPos + new Vector3(offsetDistance, 0, 0);
				fingerIconRoot.localEulerAngles = new Vector3(0, 0, 90f); // Pointing left
				break;
		}

		fingerBasePos = fingerIconRoot.localPosition;
	}

	//Animation

	private void StartFingerAnimation()
	{
		StopFingerAnimation();
		fingerAnimCoroutine = StartCoroutine(AnimateFinger());
	}

	private void StopFingerAnimation()
	{
		if (fingerAnimCoroutine != null)
		{
			StopCoroutine(fingerAnimCoroutine);
			fingerAnimCoroutine = null;
		}
	}

	private IEnumerator AnimateFinger()
	{
		float t = 0f;
		while (true)
		{
			t += Time.deltaTime * fingerBobSpeed;
			float offset = Mathf.Sin(t) * fingerBobAmount;
			fingerIconRoot.localPosition = fingerBasePos + new Vector3(0, offset, 0);
			yield return null;
		}
	}

	private IEnumerator FadeIn()
	{
		overlayCanvasGroup.alpha = 0f;
		float elapsed = 0f;
		while (elapsed < fadeSpeed)
		{
			elapsed += Time.deltaTime;
			overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeSpeed);
			yield return null;
		}
		overlayCanvasGroup.alpha = 1f;
	}

	private IEnumerator FadeOut()
	{
		float elapsed = 0f;
		while (elapsed < fadeSpeed)
		{
			elapsed += Time.deltaTime;
			overlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeSpeed);
			yield return null;
		}
		overlayCanvasGroup.alpha = 0f;
	}

	//Input

	private IEnumerator WaitForTap()
	{
		waitingForTap = true;
		// Small delay
		yield return new WaitForSeconds(0.2f);
		yield return new WaitUntil(() => !waitingForTap);
	}

	private void Update()
	{
		if (waitingForTap)
		{
			if (Input.GetKeyDown(KeyCode.Z)
				|| Input.GetKeyDown(KeyCode.Space)
				|| Input.GetMouseButtonDown(0))
			{
				waitingForTap = false;
			}
		}
	}
}
