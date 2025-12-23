using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Boss Pool")]
public class BossPool : ScriptableObject
{
	public List<BossFormation> formations = new();
}
