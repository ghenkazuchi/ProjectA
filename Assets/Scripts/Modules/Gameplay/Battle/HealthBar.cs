using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	[SerializeField] Image health;
	void Awake()
	{
		if (health == null)
		{
			health = GetComponentInChildren<Image>();
			if (health == null)
			{
				Debug.LogError("Health Image component not assigned or found!");
			}
		}
	}


	public void SetHP(float hpNormalized)
	{
		health.fillAmount = hpNormalized;
	}
}
