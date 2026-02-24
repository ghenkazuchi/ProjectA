using UnityEngine;

public class OverworldGameProgressUI : MonoBehaviour
{
	[Header("Day/Night")]
	[SerializeField] private DayNightCycleController dayNightController;

	[Header("Indicator (mũi tên)")]
	[SerializeField] private RectTransform arrowIndicator;

	[Header("Tick icons (truyền thẳng từ hierarchy)")]
	[Tooltip("26 TickIcon theo đúng thứ tự từ trái sang phải")]
	[SerializeField] private RectTransform[] tickIcons;

	[Header("Cấu hình 1 ngày (UI ↔ logic) — phải trùng DayNightCycleController")]
	[Tooltip("Số tick icon dùng cho BAN NGÀY trong 1 ngày (ví dụ 4)")]
	[SerializeField] private int dayTicks = 5;
	[Tooltip("Số tick icon dùng cho BAN ĐÊM trong 1 ngày (ví dụ 4)")]
	[SerializeField] private int nightTicks = 5;

	[Header("Boss cycle (3 ngày + 3 đêm)")]
	[SerializeField] private int cyclesToBoss = 3;

	[Header("Di chuyển mượt")]
	[SerializeField] private bool animateToTick = true;
	[SerializeField] private float moveDuration = 0.2f;

	private RectTransform _arrowRect;
	private int _currentTickIndex = -1;
	private Vector3 _currentWorldPos;
	private Vector3 _initialArrowPos;
	private float _stepX;
	private bool _initialized;

	private void Awake()
	{
		if (arrowIndicator != null)
			_arrowRect = arrowIndicator;
	}

	private void Update()
	{
		if (_arrowRect == null || tickIcons == null || tickIcons.Length == 0)
			return;

		// Tự tìm DayNight nếu chưa gán (tránh quên Bind / quên kéo ref)
		if (dayNightController == null)
			dayNightController = FindAnyObjectByType<DayNightCycleController>();
		if (dayNightController == null)
			return;

		// Khởi tạo lần đầu sau khi layout/Canvas đã ổn định (tránh lệch so với Scene)
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

		// 1 chu kỳ = 1 ngày (day + night)
		int stepsPerCycle = Mathf.Max(1, daySteps + nightSteps);
		int stepsToBoss = Mathf.Max(1, cyclesToBoss) * stepsPerCycle;
		int stepInBossWindow = totalSteps % stepsToBoss;

		// Chu kỳ thứ mấy (0..cyclesToBoss-1) và bước bên trong chu kỳ đó
		int cycleIndex = stepInBossWindow / stepsPerCycle;
		int stepInCycle = stepInBossWindow % stepsPerCycle;

		int ticksPerCycle = Mathf.Max(1, dayTicks + nightTicks);
		int localTickIndex;

		// Ban ngày: map [0 .. daySteps) -> [0 .. dayTicks-1]
		if (stepInCycle < daySteps)
		{
			float fracDay = (float)stepInCycle / Mathf.Max(1, daySteps); // 0..1
			int idxDay = Mathf.FloorToInt(fracDay * dayTicks);
			if (idxDay >= dayTicks) idxDay = dayTicks - 1;
			localTickIndex = Mathf.Max(0, idxDay);
		}
		// Ban đêm: map [daySteps .. daySteps+nightSteps) -> [dayTicks .. dayTicks+nightTicks-1]
		else
		{
			int stepNight = stepInCycle - daySteps;
			float fracNight = (float)stepNight / Mathf.Max(1, nightSteps); // 0..1
			int idxNight = Mathf.FloorToInt(fracNight * nightTicks);
			if (idxNight >= nightTicks) idxNight = nightTicks - 1;
			localTickIndex = Mathf.Max(0, dayTicks + idxNight);
		}

		int targetTickIndex = cycleIndex * ticksPerCycle + localTickIndex;
		targetTickIndex = Mathf.Clamp(targetTickIndex, 0, tickIcons.Length - 1);
		_currentTickIndex = targetTickIndex;

		// X: dùng offset từ tick 0, hoặc lấy trực tiếp từ tick nếu _stepX = 0
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
