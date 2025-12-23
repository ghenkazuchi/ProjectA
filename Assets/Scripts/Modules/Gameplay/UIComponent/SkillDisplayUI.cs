using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillDisplayUI : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI skillTextName;

	public void SetupSkillName(BaseSkillData skillData)
	{
		skillTextName.text = skillData.skillName;
	}
}
