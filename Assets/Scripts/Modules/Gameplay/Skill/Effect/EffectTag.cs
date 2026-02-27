[System.Flags]
public enum EffectTag
{
	None           = 0,
	Buff           = 1 << 0,
	Debuff         = 1 << 1,
	DoT            = 1 << 2,
	CC             = 1 << 3,
	Heal           = 1 << 4,
	Shield         = 1 << 5,
	Utility        = 1 << 6,
}
