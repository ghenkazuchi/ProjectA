using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerVision : MonoBehaviour
{
	public DayNightCycleController dayNight;
	public Light2D playerLight;

	[Header("Night Vision Radius")]
	public float nightOuterRadius = 2.5f;
	public float nightInnerRadius = 0.7f;
	public float nightIntensity = 1.0f;

	void Reset()
	{
		playerLight = GetComponentInChildren<Light2D>(true);
		dayNight = FindObjectOfType<DayNightCycleController>();
	}

	void Update()
	{
		if (!playerLight || !dayNight) return;

		bool night = dayNight.isNight;
		if (night)
		{
			playerLight.enabled = true;
			playerLight.intensity = nightIntensity;
			playerLight.pointLightOuterRadius = nightOuterRadius;
			playerLight.pointLightInnerRadius = nightInnerRadius;
		}
		else
		{
			playerLight.intensity = 0f;
			playerLight.pointLightOuterRadius = 0f;
			playerLight.pointLightInnerRadius = 0f;
			playerLight.enabled = false;
		}
	}
}
