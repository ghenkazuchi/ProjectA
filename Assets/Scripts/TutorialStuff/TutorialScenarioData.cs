using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialScenario", menuName = "Tutorial/Scenario Data")]
public class TutorialScenarioData : ScriptableObject
{
	public string scenarioTitle;
	[TextArea(2, 4)] public string scenarioDescription;
	public Sprite scenarioIcon;

	[Header("Fixed Battle Setup")]
	public List<TutorialPlayerEntry> fixedPlayerCharacters;
	public List<TutorialMonsterEntry> fixedMonsters;

	[Header("Tutorial Flow")]
	public List<TutorialStepData> steps;

	[Header("Monster Scripting")]
	public List<TutorialMonsterDirective> monsterDirectives;

	[Header("Completion")]
	[TextArea(2, 4)] public string completionMessage;
}
