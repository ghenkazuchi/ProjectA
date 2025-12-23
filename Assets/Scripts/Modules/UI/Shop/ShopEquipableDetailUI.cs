using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopEquipableDetailUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI priceText;
	[SerializeField] private List<TextMeshProUGUI> statTexts;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private Button buyButton;
	[SerializeField] private Button rerollButton;
	private EquipableBaseData currentEquipable; 

	public event System.Action<EquipableBaseData> OnBuyButtonClicked;
	public event System.Action OnRerollClicked;

	private void Awake()
	{
		buyButton.onClick.AddListener(() =>
		{
			OnBuyButtonClicked?.Invoke(currentEquipable);
		});
		rerollButton.onClick.AddListener(() =>
		{
			OnRerollClicked?.Invoke();
		});
	}
	public void SetUp(EquipableBaseData equipableBaseData)
	{
		currentEquipable = equipableBaseData;
		priceText.text = $"Price: {equipableBaseData.basePrice.ToString()}";
		descriptionText.text = equipableBaseData.description;

		for(int i = 0; i< statTexts.Count; i++)
		{
			if(equipableBaseData.EquipableStatBonus != null && i < equipableBaseData.EquipableStatBonus.Count)
			{
				var b = equipableBaseData.EquipableStatBonus[i];
				var sign = b.ModType == ModType.Percentage ? "%" : "";
				statTexts[i].text = $"{b.Stat}: {b.value}{sign}";
				statTexts[i].gameObject.SetActive(true);
			}
			else
			{
				statTexts[i].gameObject.SetActive(false);
			}
		}
	}
}
