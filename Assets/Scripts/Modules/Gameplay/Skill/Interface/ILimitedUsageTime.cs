using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILimitedUsageTime
{
	public void SetUsageTracker(EffectUsageTracker tracker);

	public bool TryConsumeUse();
}
