using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-100)]
public class DayNightCycleController : MonoBehaviour
{
	[Header("Refs")]
	public Light2D globalLight;

	[Header("Cycle Settings")]
	public int stepsPerDay = 360;
	public int currentStep = 0;
	[Range(0f, 1f)] public float time01 = 0f;

	public bool startAtNight = false;

	[Header("Ambient Intensity")]
	[Range(0f, 2f)] public float dayAmbient = 1.0f;
	[Range(0f, 2f)] public float nightAmbient = 0.06f;

	[Header("Ambient Curves/Colors")]
	public AnimationCurve ambientOverDay = AnimationCurve.EaseInOut(0, 1f, 1, 1f);
	public Gradient ambientColor = new Gradient()
	{
		colorKeys = new[] {
			new GradientColorKey(new Color(1f,.95f,.85f), 0f),
			new GradientColorKey(Color.white,            0.25f),
			new GradientColorKey(new Color(.9f,.95f,1f), 0.5f),
			new GradientColorKey(new Color(.25f,.3f,.45f), 0.75f),
			new GradientColorKey(new Color(.2f,.22f,.35f), 1f)
		}
	};

	[Header("Night Window ")]
	[Range(0f, 1f)] public float nightWindowStart = 0.70f;
	[Range(0f, 1f)] public float nightWindowEnd = 0.20f;

	public bool isNight { get; private set; }

	public event Action<bool> OnNightStateChanged;


	void Awake()
	{
		if (startAtNight)
			time01 = Mathf.Repeat(nightWindowStart + 0.001f, 1f);
		else
			time01 = 0.25f;
		UpdateNightState(true);
	}


	public void AdvanceSteps(int steps = 1)
	{
		if (stepsPerDay <= 0) return;
		currentStep += steps;
		time01 += (float)steps / Mathf.Max(1, stepsPerDay);
		if (time01 >= 1f) time01 -= 1f;
		UpdateNightState(true);
	}

	public void SetTime01(float t, bool fireEvent = true)
	{
		time01 = Mathf.Repeat(t, 1f);
		UpdateNightState(fireEvent);
	}

    void UpdateNightState(bool fireEvent)
    {
        bool nightNow = (time01 > nightWindowStart) || (time01 <= nightWindowEnd);
        if (nightNow != isNight)
        {
            isNight = nightNow;
            if (fireEvent) OnNightStateChanged?.Invoke(isNight);
        }
        else
        {
            isNight = nightNow;
        }
		if (globalLight != null)
		{
			globalLight.intensity = isNight ? nightAmbient : dayAmbient;
		}
	}

	public void AdvanceToNextMorning(bool fireEvent = true)
	{
		currentStep = 0;
		AdvanceSteps(stepsPerDay);
		SetTime01(0.25f, fireEvent);
	}

	public int GetCurrentStep() => currentStep;


	[ContextMenu("Preset: Very Dark Night")]
	void PresetVeryDarkNight()
	{
		ambientOverDay = new AnimationCurve(
			new Keyframe(0.00f, 0.90f),
			new Keyframe(0.25f, 1.00f),
			new Keyframe(0.60f, 0.60f),
			new Keyframe(0.70f, 0.06f),
			new Keyframe(0.85f, 0.05f),
			new Keyframe(1.00f, 0.90f)
		);
		nightWindowStart = 0.70f;
		nightWindowEnd = 0.20f;
		UpdateNightState(true);
	}
#if UNITY_EDITOR
	[ContextMenu("Jump To Midnight")]
	void JumpToMidnight() => SetTime01(nightWindowStart + 0.001f, true);

	[ContextMenu("Jump To Midday")]
	void JumpToMidday() => SetTime01(0.25f, true);
#endif
}
