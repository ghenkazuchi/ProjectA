using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRecruitableCharacter
{
	public abstract void InitCharacterData();
	public abstract void Recruit();
}
