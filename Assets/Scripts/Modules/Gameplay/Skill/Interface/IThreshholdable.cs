using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThreshholdable
{
	public bool CheckThreshold(int currentValue, int thresholdValue);

	public void ReachThreshold();
}
