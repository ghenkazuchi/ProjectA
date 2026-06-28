using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerCreationUITest : MonoBehaviour
{
	[System.Serializable]
	private class CharacterPortraitOption
	{
		public CharacterRaceData race = null;
		public ClassData characterClass = null;
		public Sprite portrait = null;
		public Sprite battleSprite = null;
	}

	public CanvasGroup canvasGroup;
	public TMP_Dropdown raceDropdown;
	public TMP_Dropdown classDropdown;
	public TMP_Dropdown elementalDropDown;
	public Button createButton;
	[SerializeField] private TMP_InputField playerNameInput;
	[SerializeField] private Image characterPortraitImage;
	[SerializeField] private List<CharacterPortraitOption> characterPortraitOptions = new List<CharacterPortraitOption>();

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
		SetDefaultCharacterName();
		SetListTrait();
		SetUpTraitButtons();
		SetUpSelectionPreview();
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

	private void SetDefaultCharacterName()
	{
		if (playerNameInput == null || !string.IsNullOrWhiteSpace(playerNameInput.text))
		{
			return;
		}

		playerNameInput.text = playerCharacterData.EntityName;
	}

	private void SetUpSelectionPreview()
	{
		raceDropdown.onValueChanged.AddListener(_ => {
			RefreshCharacterPreview();
			SetListTrait();
		});
		classDropdown.onValueChanged.AddListener(_ => {
			RefreshCharacterPreview();
			SetListTrait();
		});
		RefreshCharacterPreview();
	}

	private void RefreshCharacterPreview()
	{
		if (characterPortraitImage == null)
		{
			return;
		}

		Sprite portrait = ResolveSelectedPortrait();
		characterPortraitImage.sprite = portrait;
		characterPortraitImage.enabled = portrait != null;
	}
	void SetListTrait()
	{
		CharacterRaceData selectedRace = GetSelectedRace();
		ClassData selectedClass = GetSelectedClass();

		Trait[] traits = (Trait[])System.Enum.GetValues(typeof(Trait));
		for (int i = 0; i < traitTexts.Length; i++)
		{
			Trait trait = traits[i];
			int baseValue = playerCharacterData.BaseTraits.ContainsKey(trait) ? playerCharacterData.BaseTraits[trait] : 0;
			int currentValue = currentTraitValues[trait];
			
			int raceBonus = 0;
			if (selectedRace != null && selectedRace.traitBonuses != null && selectedRace.traitBonuses.ContainsKey(trait))
				raceBonus = selectedRace.traitBonuses[trait];
				
			int classBonus = 0;
			if (selectedClass != null && selectedClass.traitBonuses != null && selectedClass.traitBonuses.ContainsKey(trait))
				classBonus = selectedClass.traitBonuses[trait];

			int totalBonus = raceBonus + classBonus;
			int finalValue = currentValue + totalBonus;

			if (currentValue > baseValue || totalBonus != 0)
			{
				traitTexts[i].color = Color.red;
			}
			else
			{
				traitTexts[i].color = Color.black;
			}
			
			if (totalBonus > 0)
			{
				traitTexts[i].text = $"{trait}: {finalValue} <color=#006400>(+{totalBonus})</color>";
			}
			else if (totalBonus < 0)
			{
				traitTexts[i].text = $"{trait}: {finalValue} <color=#8B0000>({totalBonus})</color>";
			}
			else
			{
				traitTexts[i].text = $"{trait}: {finalValue}";
			}
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

	private CharacterRaceData GetSelectedRace()
	{
		if (allRaces == null || allRaces.Count == 0)
		{
			return null;
		}

		int raceIndex = Mathf.Clamp(raceDropdown.value, 0, allRaces.Count - 1);
		return allRaces[raceIndex];
	}

	private ClassData GetSelectedClass()
	{
		if (allClasses == null || allClasses.Count == 0)
		{
			return null;
		}

		int classIndex = Mathf.Clamp(classDropdown.value, 0, allClasses.Count - 1);
		return allClasses[classIndex];
	}

	private Sprite ResolveSelectedPortrait()
	{
		CharacterPortraitOption option = ResolveSelectedVisualOption();
		if (option != null && option.portrait != null)
		{
			return option.portrait;
		}

		if (playerCharacterData.EntityPortrait != null)
		{
			return playerCharacterData.EntityPortrait;
		}

		return playerCharacterData.EntitySprite;
	}

	private Sprite ResolveSelectedBattleSprite()
	{
		CharacterPortraitOption option = ResolveSelectedVisualOption();
		if (option != null && option.battleSprite != null)
		{
			return option.battleSprite;
		}

		return playerCharacterData.EntitySprite;
	}

	private CharacterPortraitOption ResolveSelectedVisualOption()
	{
		CharacterRaceData selectedRace = GetSelectedRace();
		ClassData selectedClass = GetSelectedClass();

		foreach (CharacterPortraitOption option in characterPortraitOptions)
		{
			if (option == null || option.race != selectedRace || option.characterClass != selectedClass)
			{
				continue;
			}

			return option;
		}

		return null;
	}

	private string ResolveCharacterName()
	{
		if (playerNameInput == null)
		{
			return playerCharacterData.EntityName;
		}

		string enteredName = playerNameInput.text.Trim();
		return string.IsNullOrWhiteSpace(enteredName) ? playerCharacterData.EntityName : enteredName;
	}

	void CreateCharacter()
	{
		StartCoroutine(CreateCharacterRoutine());
	}

	[SerializeField] private TextMeshProUGUI statusText;

	IEnumerator CreateCharacterRoutine()
	{
		canvasGroup.interactable = false;
		var mapGen = FindObjectOfType<VoronoiPathGenerator>();
		if (mapGen != null && !mapGen.IsMapGenerated())
		{
			if (statusText != null) statusText.text = "Generating World...";
			yield return new WaitUntil(() => mapGen.IsMapGenerated());
			if (statusText != null) statusText.text = "";
		}

		CharacterRaceData race = GetSelectedRace();
		ClassData cls = GetSelectedClass();
		if (race == null || cls == null)
		{
			yield return null;
		}

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

		RunTimeEntityData runtimeData = ScriptableObject.CreateInstance<RunTimeEntityData>();
		runtimeData.CloneFrom(playerCharacterData);
		runtimeData.EntityName = ResolveCharacterName();
		runtimeData.EntityPortrait = ResolveSelectedPortrait();
		runtimeData.EntitySprite = ResolveSelectedBattleSprite();
		runtimeData.SetTraits(currentTraitValues);
		runtimeData.EntityElement = chosenElement;
		PlayerCharacter newCharacter = new PlayerCharacter(cls, race,1,runtimeData);
		newCharacter.IsCreatedCharacter = true;
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
