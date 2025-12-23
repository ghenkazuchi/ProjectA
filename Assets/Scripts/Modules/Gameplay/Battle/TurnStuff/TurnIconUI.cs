using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnIconUI : MonoBehaviour
{
	[SerializeField] private Image avatar;
	[SerializeField] private CanvasGroup cg;
	[SerializeField] private RectTransform rt;

	void Awake()
	{
		if (cg == null) cg = GetComponent<CanvasGroup>();
		if (rt == null) rt = GetComponent<RectTransform>();
	}

	public void SetData(EntityBase entity)
	{
		avatar.sprite = entity.entityData.EntitySprite;
	}
}
