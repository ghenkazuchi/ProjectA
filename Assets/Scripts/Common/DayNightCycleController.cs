using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-100)]
public class DayNightCycleController : MonoBehaviour
{
	[Header("Refs")]
	public Light2D globalLight;

	[Header("Cycle Settings")]
	[Tooltip("Total steps for one Day+Night cycle. If 'Fixed Day/Night Steps' is enabled, this value will automatically equal daySteps + nightSteps.")]
	public int stepsPerDay = 200;
	public int currentStep = 0;
	[Range(0f, 1f)] public float time01 = 0f;

	public bool startAtNight = false;

	[Header("He-is-coming style (time advances on movement)")]
	[SerializeField] private bool useFixedDayNightSteps = true;
	public int daySteps = 100;
	public int nightSteps = 100;

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
	public event Action<int, int> OnStepsAdvanced; // (deltaSteps, totalSteps)


	void Awake()
	{
		ApplyFixedScheduleIfNeeded();
		if (startAtNight)
		{
			time01 = Mathf.Repeat(nightWindowStart + 0.001f, 1f);
			if (useFixedDayNightSteps) currentStep = daySteps;
		}
		else
		{
			time01 = 0f;
			currentStep = 0;
		}
		UpdateNightState(true);
		if (HaKien.MessageManager.Instance != null)
		{
			HaKien.MessageManager.Instance.SendMessage(new HaKien.Message(HaKien.MessageType.OnTimeChanged, new object[] { isNight }));
		}
	}

	private void OnValidate()
	{
		ApplyFixedScheduleIfNeeded();
	}

	private void ApplyFixedScheduleIfNeeded()
	{
		if (!useFixedDayNightSteps) return;
		daySteps = Mathf.Max(1, daySteps);
		nightSteps = Mathf.Max(1, nightSteps);
		stepsPerDay = daySteps + nightSteps;

		// Night = end of the cycle: [dayEnd..1)
		nightWindowStart = Mathf.Clamp01((float)daySteps / Mathf.Max(1, stepsPerDay));
		nightWindowEnd = 1f;
	}

	public void AdvanceSteps(int steps = 1)
	{
		if (stepsPerDay <= 0) return;
		int delta = Mathf.Max(0, steps);
		if (delta == 0) return;

		currentStep += delta;
		time01 += (float)delta / Mathf.Max(1, stepsPerDay);
		if (time01 >= 1f) time01 -= 1f;
		UpdateNightState(true);
		OnStepsAdvanced?.Invoke(delta, currentStep);
	}

	public void SetTime01(float t, bool fireEvent = true)
	{
		time01 = Mathf.Repeat(t, 1f);
		UpdateNightState(fireEvent);
	}

    void UpdateNightState(bool fireEvent)
    {
		bool nightNow;
		// Supports both non-wrap windows (start < end) and wrap windows (start > end)
		if (nightWindowStart <= nightWindowEnd)
		{
			nightNow = (time01 >= nightWindowStart) && (time01 < nightWindowEnd);
		}
		else
		{
			nightNow = (time01 > nightWindowStart) || (time01 <= nightWindowEnd);
		}
        if (nightNow != isNight)
        {
            isNight = nightNow;
            if (fireEvent)
            {
                OnNightStateChanged?.Invoke(isNight);
                if (HaKien.MessageManager.Instance != null)
                {
                    HaKien.MessageManager.Instance.SendMessage(new HaKien.Message(HaKien.MessageType.OnTimeChanged, new object[] { isNight }));
                }
            }
        }
        else
        {
            isNight = nightNow;
        }

		// Jump color and light abruptly, skip gradient transition based on time01
		if (globalLight != null)
		{
			globalLight.intensity = isNight ? nightAmbient : dayAmbient;
			globalLight.color = isNight ? ambientColor.Evaluate(1f) : ambientColor.Evaluate(0f);
		}
	}

	public void AdvanceToNextMorning(bool fireEvent = true)
	{
		// Advance to the start of the next day cycle (morning)
		int completedDays = (currentStep / Mathf.Max(1, stepsPerDay)) + 1;
		currentStep = completedDays * stepsPerDay;
		SetTime01(0f, fireEvent);
		OnStepsAdvanced?.Invoke(0, currentStep);
	}

}
