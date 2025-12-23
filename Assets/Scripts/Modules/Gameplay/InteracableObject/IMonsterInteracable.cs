using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonsterInteracable
{
	public abstract void StartBattle();
	public abstract void PrepareMonster();
	public abstract void Defeated();
}
