using UnityEngine;
using UnityEngine.UI;

public class OverworldGameProgressUI : MonoBehaviour
{
	[SerializeField] private DayNightCycleController dayNightController;

	[SerializeField] private Image bossIconImage;

	[SerializeField] private RectTransform arrowIndicator;

	[SerializeField] private RectTransform[] tickIcons;

	[SerializeField] private int dayTicks = 5;
	[SerializeField] private int nightTicks = 5;

	[SerializeField] private int cyclesToBoss = 3;
	[SerializeField] private bool animateToTick = true;
	[SerializeField] private float moveDuration = 0.2f;

	private RectTransform _arrowRect;
	private int _currentTickIndex = -1;
	private Vector3 _currentWorldPos;
	private Vector3 _initialArrowPos;
	private float _stepX;
	private bool _initialized;
	private BossManager _bossManager;

	private void OnEnable()
	{
		_bossManager = FindAnyObjectByType<BossManager>();
		if (_bossManager != null)
		{
			_bossManager.OnBossRolled += UpdateBossIcon;
			if (_bossManager.currentBoss != null)
			{
				UpdateBossIcon(_bossManager.currentBoss);
			}
		}
	}

	private void OnDisable()
	{
		if (_bossManager != null)
		{
			_bossManager.OnBossRolled -= UpdateBossIcon;
		}
	}

	private void UpdateBossIcon(BossFormation formation)
	{
		if (bossIconImage != null && formation != null)
		{
			bossIconImage.sprite = formation.bossIcon;
			bossIconImage.enabled = formation.bossIcon != null;
		}
	}

	private void Awake()
	{
		if (arrowIndicator != null)
			_arrowRect = arrowIndicator;
	}

	private void Update()
	{
		if (_arrowRect == null || tickIcons == null || tickIcons.Length == 0)
			return;
		if (dayNightController == null)
			dayNightController = FindAnyObjectByType<DayNightCycleController>();
		if (dayNightController == null)
			return;

		if (!_initialized)
		{
			_initialArrowPos = _arrowRect.position;
			if (tickIcons.Length >= 2)
			{
				_stepX = tickIcons[1].position.x - tickIcons[0].position.x;
				if (Mathf.Approximately(_stepX, 0f) && tickIcons.Length > 1)
					_stepX = (tickIcons[tickIcons.Length - 1].position.x - tickIcons[0].position.x) / (tickIcons.Length - 1);
			}
			else
				_stepX = 0f;

			_currentWorldPos = _initialArrowPos;
			_initialized = true;
		}

		int totalSteps = dayNightController.currentStep;

		int daySteps = dayNightController.daySteps;
		int nightSteps = dayNightController.nightSteps;

		int stepsPerCycle = Mathf.Max(1, daySteps + nightSteps);
		int stepsToBoss = Mathf.Max(1, cyclesToBoss) * stepsPerCycle;
		int stepInBossWindow = totalSteps % stepsToBoss;

		int cycleIndex = stepInBossWindow / stepsPerCycle;
		int stepInCycle = stepInBossWindow % stepsPerCycle;

		int ticksPerCycle = Mathf.Max(1, dayTicks + nightTicks);
		int localTickIndex;

		if (stepInCycle < daySteps)
		{
			float fracDay = (float)stepInCycle / Mathf.Max(1, daySteps); 
			int idxDay = Mathf.FloorToInt(fracDay * dayTicks);
			if (idxDay >= dayTicks) idxDay = dayTicks - 1;
			localTickIndex = Mathf.Max(0, idxDay);
		}
		else
		{
			int stepNight = stepInCycle - daySteps;
			float fracNight = (float)stepNight / Mathf.Max(1, nightSteps); 
			int idxNight = Mathf.FloorToInt(fracNight * nightTicks);
			if (idxNight >= nightTicks) idxNight = nightTicks - 1;
			localTickIndex = Mathf.Max(0, dayTicks + idxNight);
		}

		int targetTickIndex = cycleIndex * ticksPerCycle + localTickIndex;
		targetTickIndex = Mathf.Clamp(targetTickIndex, 0, tickIcons.Length - 1);
		_currentTickIndex = targetTickIndex;

		float targetX;
		if (!Mathf.Approximately(_stepX, 0f))
			targetX = _initialArrowPos.x + _currentTickIndex * _stepX;
		else
			targetX = tickIcons[_currentTickIndex].position.x;
		Vector3 targetPos = new Vector3(targetX, _initialArrowPos.y, _initialArrowPos.z);

		if (animateToTick && moveDuration > 0f)
		{
			float dist = Vector3.Distance(_currentWorldPos, targetPos);
			float maxDelta = (dist / Mathf.Max(0.0001f, moveDuration)) * Time.deltaTime;
			_currentWorldPos = Vector3.MoveTowards(_currentWorldPos, targetPos, maxDelta);
		}
		else
		{
			_currentWorldPos = targetPos;
		}

		UpdateArrowPosition(_currentWorldPos);
	}

	private void UpdateArrowPosition(Vector3 worldPos)
	{
		_arrowRect.position = worldPos;
	}

	public void Bind(DayNightCycleController controller)
	{
		dayNightController = controller;
	}
}
