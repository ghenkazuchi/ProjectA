using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerCreationUITest : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public TMP_Dropdown raceDropdown;
	public TMP_Dropdown classDropdown;
	public TMP_Dropdown elementalDropDown;
	public Button createButton;

	public List<CharacterRaceData> allRaces;
	public List<ClassData> allClasses;

	private GameObject playerObject;
	[SerializeField] private BaseEntityData playerCharacterData;
	public TextMeshProUGUI[] traitTexts;
	public Button[] traitPlusButtons;
	public Button[] traitMinusButtons;
	public TextMeshProUGUI bonusPointText;
	public int bonusTraitPoints;
	private Trait[] traits;
	private Dictionary<Trait,int> currentTraitValues = new Dictionary<Trait,int>();

	private void Awake()
	{
		GameController.Instance.currentState = GameState.CharacterCreation;
		MessageManager.Instance.SendMessage(new Message(MessageType.OnCharacterCreationEnter));
	}
	void Start()
	{
		bonusTraitPoints = Random.Range(5, 11);
		bonusPointText.text += bonusTraitPoints.ToString();
		playerObject = GameObject.Find("Player");
		if (playerObject == null)
		{
			return;
		}
		traits = (Trait[])System.Enum.GetValues(typeof(Trait));
		foreach (Trait trait in traits)
		{
			int baseValue = playerCharacterData.BaseTraits.ContainsKey(trait) ? playerCharacterData.BaseTraits[trait] : 0;
			currentTraitValues[trait] = baseValue;
		}
		PopulateDropdowns();
		PopulateElementDropDown();
		SetListTrait();
		SetUpTraitButtons();
		createButton.onClick.AddListener(CreateCharacter);
	}

	void PopulateDropdowns()
	{
		raceDropdown.ClearOptions();
		raceDropdown.AddOptions(allRaces.ConvertAll(r => r.name));

		classDropdown.ClearOptions();
		classDropdown.AddOptions(allClasses.ConvertAll(c => c.name));
	}

	private void PopulateElementDropDown()
	{
		elementalDropDown.ClearOptions();
		var options = new List<string>();	
		foreach(Element e in System.Enum.GetValues(typeof(Element)))
		{
			if(e == Element.None) continue;
			options.Add(e.ToString());
		}
		elementalDropDown.AddOptions(options);
	}
	void SetListTrait()
	{
		Trait[] traits = (Trait[])System.Enum.GetValues(typeof(Trait));
		for (int i = 0; i < traitTexts.Length; i++)
		{
			Trait trait = traits[i];
			int baseValue = playerCharacterData.BaseTraits.ContainsKey(trait) ? playerCharacterData.BaseTraits[trait] : 0;
			int currentValue = currentTraitValues[trait];
			if (currentValue > baseValue)
			{
				traitTexts[i].color = Color.red;
			}
			else
			{
				traitTexts[i].color = Color.black;
			}
			traitTexts[i].text = $"{trait}: {currentValue}";
		}
		bonusPointText.text = $"Bonus Points: {bonusTraitPoints}";
	}
	void SetUpTraitButtons()
	{
		for (int i = 0; i < traitPlusButtons.Length && i < traits.Length; i++)
		{
			int index = i;
			traitPlusButtons[i].onClick.AddListener(() => ModifyTrait(traits[index], 1));
			traitMinusButtons[i].onClick.AddListener(() => ModifyTrait(traits[index], -1));
		}
	}
	void ModifyTrait(Trait trait, int delta)
	{
		int baseValue = playerCharacterData.BaseTraits.ContainsKey(trait) ? playerCharacterData.BaseTraits[trait] : 0;
		int currentValue = currentTraitValues[trait];
		if (delta > 0)
		{
			if (bonusTraitPoints <= 0) return;

			currentTraitValues[trait]++;
			bonusTraitPoints--;
		}
		else if (delta < 0)
		{
			if (currentValue <= baseValue) return;

			currentTraitValues[trait]--;
			bonusTraitPoints++;
		}

		SetListTrait();
	}

	void CreateCharacter()
	{
		int raceIndex = raceDropdown.value;
		int classIndex = classDropdown.value;
		Element chosenElement = Element.None;
		{
			var values = new List<Element>();
			foreach (Element e in System.Enum.GetValues(typeof(Element)))
			{
				if(e != Element.None)
					values.Add(e);
			}
			chosenElement = values[elementalDropDown.value];
		}
			CharacterRaceData race = allRaces[raceIndex];
		ClassData cls = allClasses[classIndex];
		RunTimeEntityData runtimeData = ScriptableObject.CreateInstance<RunTimeEntityData>();
		runtimeData.EntityName = playerCharacterData.EntityName;
		runtimeData.EntitySprite = playerCharacterData.EntitySprite;
		runtimeData.CloneFrom(playerCharacterData);
		runtimeData.SetTraits(currentTraitValues);
		runtimeData.EntityElement = chosenElement;
		PlayerCharacter newCharacter = new PlayerCharacter(cls, race,1,runtimeData);
		newCharacter.InitializeEntity(1);

		PlayerParty party = playerObject.GetComponent<PlayerParty>();
		party.AddPartyMember(newCharacter, new GridPosition(0, 1));
		Debug.Log($" Race: {race.name}, Class: {cls.name}");
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		GameController.Instance.currentState = GameState.FreeRoam;
		MessageManager.Instance.SendMessage(new Message(MessageType.OnGameStart));
	}
}
