using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBeforeStatusApplied
{
	IEnumerator OnBeforeStatusApplied(StatusApplyContext context);
}
