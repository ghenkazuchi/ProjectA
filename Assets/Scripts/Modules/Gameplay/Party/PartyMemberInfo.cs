using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class PartyMemberInfo : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler
{
	[SerializeField] EntityBase characterData;
	[SerializeField] TextMeshProUGUI characterNameText;
	[SerializeField] TextMeshProUGUI characterLevelText;
	[SerializeField] Image characterHPFill;
	[SerializeField] Image characterMPFill;
	[SerializeField] Image characterSPFill;

	private GridPosition gridPosition;
	private PartyMenuController partyMenuController;
	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;
	private Canvas canvas;
	private Vector3 originalPosition;
	private Transform originalParent;
	private bool isDragging = false;
	private int originalSiblingIndex;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = gameObject.AddComponent<CanvasGroup>();

		rectTransform = GetComponent<RectTransform>();

		canvas = GetComponentInParent<Canvas>();
		if (canvas == null)
		{
			Canvas[] canvases = FindObjectsOfType<Canvas>();
			foreach (var c in canvases)
			{
				if (c.name.Contains("Party"))
				{
					canvas = c;
					break;
				}
			}
		}
		Debug.Log($"PartyMemberInfo Awake: {gameObject.name}, Canvas: {canvas?.name}");
	}

	public void Initialize(GridPosition position, PartyMenuController controller)
	{
		gridPosition = position;
		partyMenuController = controller;

		Debug.Log($"Initialize: {gameObject.name} at position ({position.x},{position.y})");
		Debug.Log($"Controller: {controller?.gameObject.name}");

		UpdateDisplay();
	}

	public void SetCharacterData(EntityBase entity)
	{
		characterData = entity;
		Debug.Log($"SetCharacterData: {gameObject.name} -> {entity?.entityData.EntityName}");
	}

	public EntityBase GetCharacterData()
	{
		return characterData;
	}

	public GridPosition GetPosition()
	{
		return gridPosition;
	}

	public bool IsEmpty()
	{
		return characterData == null;
	}

	private void UpdateDisplay()
	{
		if (characterData != null)
		{
			// Show character info
			characterNameText.text = characterData.entityData.EntityName;
			characterLevelText.text = $"Lv.{characterData.Level}";

			float hpRatio = (float)characterData.GetCurrentHP() / characterData.MaxHp;
			characterHPFill.fillAmount = hpRatio;

			float mpRatio = (float)characterData.GetCurrentMP() / characterData.MaxMP;
			characterMPFill.fillAmount = mpRatio;

			float spRatio = (float)characterData.GetCurrentSP() / characterData.MaxSP;
			characterSPFill.fillAmount = spRatio;

			characterNameText.gameObject.SetActive(true);
			characterLevelText.gameObject.SetActive(true);
			characterHPFill.transform.parent.gameObject.SetActive(true);
			characterMPFill.transform.parent.gameObject.SetActive(true);
			characterSPFill.transform.parent.gameObject.SetActive(true);
		}
		else
		{
			// Hide character info
			characterNameText.text = "Empty Slot";
			characterLevelText.text = "";
			characterHPFill.fillAmount = 0;
			characterMPFill.fillAmount = 0;
			characterSPFill.fillAmount = 0;

			characterLevelText.gameObject.SetActive(false);
			characterHPFill.transform.parent.gameObject.SetActive(false);
			characterMPFill.transform.parent.gameObject.SetActive(false);
			characterSPFill.transform.parent.gameObject.SetActive(false);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Debug.Log($"=== OnBeginDrag: {gameObject.name} ===");
		Debug.Log($"Character: {characterData?.entityData.EntityName}");
		Debug.Log($"Position: ({gridPosition.x},{gridPosition.y})");
		Debug.Log($"PartyMenuController: {partyMenuController?.gameObject.name}");

		if (characterData == null)
		{
			Debug.Log("No character data - drag cancelled");
			return;
		}

		isDragging = true;
		originalPosition = rectTransform.position;
		originalParent = rectTransform.parent;
		originalSiblingIndex = rectTransform.GetSiblingIndex();

		canvasGroup.alpha = 0.6f;
		canvasGroup.blocksRaycasts = false;

		if (canvas != null)
		{
			rectTransform.SetParent(canvas.transform);
			rectTransform.SetAsLastSibling();
			Debug.Log($"Moved to canvas: {canvas.name}");
		}
		else
		{
			Debug.LogError("Canvas is null!");
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging) return;

		rectTransform.position = eventData.position;
	}

	public void OnDrop(PointerEventData eventData)
	{
		Debug.Log($"=== OnDrop: {gameObject.name} ===");
		Debug.Log($"This position: ({gridPosition.x},{gridPosition.y})");
		Debug.Log($"This character: {characterData?.entityData.EntityName}");

		if (partyMenuController == null)
		{
			Debug.LogError("PartyMenuController is null!");
			return;
		}

		PartyMemberInfo draggedItem = eventData.pointerDrag?.GetComponent<PartyMemberInfo>();

		if (draggedItem == null)
		{
			Debug.LogError("Dragged item is null!");
			return;
		}

		Debug.Log($"Dragged item: {draggedItem.gameObject.name}");
		Debug.Log($"Dragged character: {draggedItem.characterData?.entityData.EntityName}");
		Debug.Log($"Dragged from: ({draggedItem.gridPosition.x},{draggedItem.gridPosition.y})");

		if (draggedItem != this)
		{
			Debug.Log("*** CALLING SWAP ***");
			partyMenuController.SwapPartyMembers(draggedItem.gridPosition, this.gridPosition);
		}
		else
		{
			Debug.Log("Same item - no swap needed");
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Debug.Log($"=== OnEndDrag: {gameObject.name} ===");

		if (!isDragging) return;

		isDragging = false;
		canvasGroup.alpha = 1f;
		canvasGroup.blocksRaycasts = true;

		if (originalParent != null)
		{
			rectTransform.SetParent(originalParent);
			rectTransform.position = originalPosition;
			rectTransform.SetSiblingIndex(originalSiblingIndex);
			Debug.Log("Restored to original position");
		}
	}

	public void RefreshDisplay()
	{
		UpdateDisplay();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (characterData != null)
		{
			Debug.Log("Message Send");
			MessageManager.Instance.SendMessage(new Message(MessageType.OnPartyMemberInfoUpdate, new object[] { characterData }));
		} 
	}
}