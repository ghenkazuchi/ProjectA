using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Day Progress Info", menuName = "DayProgress/Day Progress Info")]
public class DayProgressInfo : ScriptableObject
{
	public GameDay day;
	public int bossLevel;
	public int minionMaxLevel;
	public int minionMinLevel;
}
