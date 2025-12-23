using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameProgressionUI : MonoBehaviour
{
	[SerializeField] private DayNightCycleController clock;
	[SerializeField] private BossManager boss;

	[SerializeField] private TextMeshProUGUI dayText;
	[SerializeField] private TextMeshProUGUI timeText;
	[SerializeField] private TextMeshProUGUI stepText;
	[SerializeField] private Image bossIcon;

	void Start()
	{
		if (boss != null)
		{
			if (boss.currentBoss != null) bossIcon.sprite = boss.currentBoss.bossIcon;
			boss.OnBossRolled += b => { if (bossIcon) bossIcon.sprite = b?.bossIcon; };
		}
	}
	void OnDestroy()
	{
		if (boss != null) boss.OnBossRolled -= b => { if (bossIcon) bossIcon.sprite = b?.bossIcon; };
	}

	void Update()
	{
		if (clock == null || boss == null) return;

		dayText.text = $"Day {Mathf.Clamp(boss.dayIndex, 1, 3)}";
		timeText.text = clock.isNight ? "Night" : "Day";
		stepText.text = $"{clock.currentStep}/{Mathf.Max(1, clock.stepsPerDay)}";
	}
}
