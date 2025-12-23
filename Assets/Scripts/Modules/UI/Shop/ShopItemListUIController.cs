using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemListUIController : MonoBehaviour
{
	[SerializeField] private ShopItemUI shopItemPrefab;
	[SerializeField] private Transform itemListTransform;
	[SerializeField] private CanvasGroup canvasGroup;

	public void Awake()
	{
		Hide();
	}

	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
	}

	public void Show()
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
	}

	public void InitShopItemList(List<EquipableBaseData> equipableList)
	{
		foreach (Transform child in itemListTransform)
		{
			Destroy(child.gameObject);
		}
		foreach (EquipableBaseData equipable in equipableList)
		{
			ShopItemUI shopItemUI = Instantiate(shopItemPrefab, itemListTransform);
			shopItemUI.SetUp(equipable);
		}
	}
}
