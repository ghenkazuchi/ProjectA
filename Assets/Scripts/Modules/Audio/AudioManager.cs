using System.Collections;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>, IMessageHandle
{
	[Header("BGM")]
	[SerializeField] private AudioSource bgmSource;
	[SerializeField] private AudioClip roamingDayBGM;
	[SerializeField] private AudioClip roamingNightBGM;
	[SerializeField] private AudioClip characterCreationBGM;
	[SerializeField] private AudioClip battleBgm;
	[SerializeField] private AudioClip campBgm;
	[SerializeField] private AudioClip shopBgm;
	[SerializeField] private AudioClip mainMenuBGM;

	[Header("Sound Event Mapping")]
	[SerializeField] private SoundEventMap eventMap;

	[Header("SFX Polyphonic Pool Settings")]
	[SerializeField] private AudioSource sfxSource; // Fallback source
	[SerializeField] private int maxPoolSize = 16;
	private List<AudioSource> sfxPool = new List<AudioSource>();

	private enum BGMState
	{
		None,
		MainMenu,
		CharacterCreation,
		Roaming,
		Battle,
		Camp,
		Shop
	}
	private BGMState currentBGMState = BGMState.None;
	private bool isNight = false;

	[Header("Battle Cast SFX Defaults")]
	[SerializeField] private AudioConfig defaultPhysCastSFX;
	[SerializeField] private AudioConfig defaultSpellCastSFX;
	[SerializeField] private AudioConfig fireCastSFX;
	[SerializeField] private AudioConfig waterCastSFX;
	[SerializeField] private AudioConfig windCastSFX;
	[SerializeField] private AudioConfig earthCastSFX;
	[SerializeField] private AudioConfig lightCastSFX;
	[SerializeField] private AudioConfig darkCastSFX;



	[Header("Battle General SFX Defaults")]
	[SerializeField] private AudioConfig defaultDamageTakenSFX;
	[SerializeField] private AudioConfig defaultDeathSFX;

	[Header("Status Effect SFX Defaults")]
	[SerializeField] private AudioConfig poisonApplySFX;
	[SerializeField] private AudioConfig sleepApplySFX;
	[SerializeField] private AudioConfig charmApplySFX;
	[SerializeField] private AudioConfig stunApplySFX;
	[SerializeField] private AudioConfig burnApplySFX;
	[SerializeField] private AudioConfig bleedApplySFX;

	[Header("Volume")]
	[Range(0f, 1f)] public float bgmVolume = 0.6f;
	[Range(0f, 1f)] public float sfxVolume = 1f;

	#region Unity lifecycle

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (bgmSource != null) bgmSource.loop = true;
		ApplyVolume();
	}

	private void OnEnable()
	{
		var mm = MessageManager.Instance;
		mm.AddSubcriber(MessageType.OnCharacterCreationEnter, this);
		mm.AddSubcriber(MessageType.OnGameStart, this);
		mm.AddSubcriber(MessageType.OnGameWin, this);
		mm.AddSubcriber(MessageType.OnGameLose, this);
		mm.AddSubcriber(MessageType.OnBattleStart, this);
		mm.AddSubcriber(MessageType.OnBattleOver, this);

		mm.AddSubcriber(MessageType.OnButtonClick, this);
		mm.AddSubcriber(MessageType.OnChestOpen, this);
		mm.AddSubcriber(MessageType.OnShopOpen, this);
		mm.AddSubcriber(MessageType.OnShopClose, this);
		mm.AddSubcriber(MessageType.OnInteract, this);
		mm.AddSubcriber(MessageType.OnSkillActive, this);
		mm.AddSubcriber(MessageType.OnInteractEnd, this);
		mm.AddSubcriber(MessageType.OnFireCampPopupOpen, this);

		mm.AddSubcriber(MessageType.OnMainMenuEnter, this);
		mm.AddSubcriber(MessageType.OnTimeChanged, this);
		mm.AddSubcriber(MessageType.OnChestOpenAnimationComplete, this);
		mm.AddSubcriber(MessageType.OnChestGoldReward, this);
		mm.AddSubcriber(MessageType.OnEquipmentEquipped, this);
		mm.AddSubcriber(MessageType.OnToastShown, this);
	}

	private void OnDisable()
	{
		var mm = MessageManager.Instance;
		if (mm == null) return;

		mm.RemoveSubcriber(MessageType.OnCharacterCreationEnter, this);
		mm.RemoveSubcriber(MessageType.OnGameStart, this);
		mm.RemoveSubcriber(MessageType.OnGameWin, this);
		mm.RemoveSubcriber(MessageType.OnGameLose, this);
		mm.RemoveSubcriber(MessageType.OnBattleStart, this);
		mm.RemoveSubcriber(MessageType.OnBattleOver, this);

		mm.RemoveSubcriber(MessageType.OnButtonClick, this);
		mm.RemoveSubcriber(MessageType.OnChestOpen, this);
		mm.RemoveSubcriber(MessageType.OnShopOpen, this);
		mm.RemoveSubcriber(MessageType.OnShopClose, this);
		mm.RemoveSubcriber(MessageType.OnInteract, this);
		mm.RemoveSubcriber(MessageType.OnSkillActive, this);
		mm.RemoveSubcriber(MessageType.OnInteractEnd, this);
		mm.RemoveSubcriber(MessageType.OnFireCampPopupOpen, this);

		mm.RemoveSubcriber(MessageType.OnMainMenuEnter, this);
		mm.RemoveSubcriber(MessageType.OnTimeChanged, this);
		mm.RemoveSubcriber(MessageType.OnChestOpenAnimationComplete, this);
		mm.RemoveSubcriber(MessageType.OnChestGoldReward, this);
		mm.RemoveSubcriber(MessageType.OnEquipmentEquipped, this);
		mm.RemoveSubcriber(MessageType.OnToastShown, this);
	}

	#endregion

	#region IMessageHandle

	public void Handle(Message message)
	{
		// 1. Manage BGM changes
		switch (message.type)
		{
			case MessageType.OnCharacterCreationEnter:
				PlayBGMState(BGMState.CharacterCreation);
				break;
			case MessageType.OnGameStart:
				PlayBGMState(BGMState.Roaming);
				break;
			case MessageType.OnBattleStart:
				PlayBGMState(BGMState.Battle);
				break;
			case MessageType.OnBattleOver:
				PlayBGMState(BGMState.Camp);
				break;
			case MessageType.OnShopOpen:
				PlayBGMState(BGMState.Shop);
				break;
			case MessageType.OnShopClose:
				PlayBGMState(BGMState.Camp);
				break;
			case MessageType.OnInteractEnd:
				PlayBGMState(BGMState.Roaming);
				break;
			case MessageType.OnFireCampPopupOpen:
				PlayBGMState(BGMState.Camp);
				break;
			case MessageType.OnMainMenuEnter:
				PlayBGMState(BGMState.MainMenu);
				break;
			case MessageType.OnTimeChanged:
				if (message.data != null && message.data.Length > 0 && message.data[0] is bool nightState)
				{
					isNight = nightState;
					if (currentBGMState == BGMState.Roaming)
					{
						PlayBGMState(BGMState.Roaming);
					}
				}
				break;
		}

		// 2. Play event-mapped SFX
		if (eventMap != null && eventMap.TryGetConfig(message.type, out AudioConfig sfxConfig))
		{
			PlaySFX(sfxConfig);
		}
	}

	#endregion

	#region Public API

	public void SetBgmVolume(float value)
	{
		bgmVolume = Mathf.Clamp01(value);
		if (bgmSource != null)
			bgmSource.volume = bgmVolume;
	}

	public void SetSfxVolume(float value)
	{
		sfxVolume = Mathf.Clamp01(value);
		ApplyVolume();
	}

	public void PlaySFX(AudioConfig config)
	{
		if (config == null) return;
		AudioClip clip = config.GetRandomClip();
		if (clip == null) return;

		AudioSource source = GetAvailableAudioSource();
		if (source != null)
		{
			source.clip = clip;
			source.volume = config.GetRandomVolume() * sfxVolume;
			source.pitch = config.GetRandomPitch();
			source.spatialBlend = 0f; // Force 2D in this game
			source.loop = false;
			source.Play();
		}
	}

	public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
	{
		if (clip == null) return;

		AudioSource source = GetAvailableAudioSource();
		if (source != null)
		{
			source.clip = clip;
			source.volume = volume * sfxVolume;
			source.pitch = pitch;
			source.spatialBlend = 0f; // Force 2D in this game
			source.loop = false;
			source.Play();
		}
	}

	#region Battle SFX Fallback Triggers

	public void PlayDefaultCastSFX(SkillDefinition definition, Element element)
	{
		if (definition == SkillDefinition.Spell)
		{
			AudioConfig config = element switch
			{
				Element.Fire => fireCastSFX,
				Element.Water => waterCastSFX,
				Element.Wind => windCastSFX,
				Element.Earth => earthCastSFX,
				Element.Light => lightCastSFX,
				Element.Dark => darkCastSFX,
				_ => defaultSpellCastSFX
			};
			PlaySFX(config != null ? config : defaultSpellCastSFX);
		}
		else
		{
			PlaySFX(defaultPhysCastSFX);
		}
	}



	public void PlayDefaultDamageSFX()
	{
		PlaySFX(defaultDamageTakenSFX);
	}

	public void PlayDefaultDeathSFX()
	{
		PlaySFX(defaultDeathSFX);
	}

	public void PlayDefaultStatusApplySFX(string effectName)
	{
		if (string.IsNullOrEmpty(effectName)) return;

		string lowerName = effectName.ToLower();
		AudioConfig config = null;

		if (lowerName.Contains("poison")) config = poisonApplySFX;
		else if (lowerName.Contains("sleep")) config = sleepApplySFX;
		else if (lowerName.Contains("charm")) config = charmApplySFX;
		else if (lowerName.Contains("stun")) config = stunApplySFX;
		else if (lowerName.Contains("burn")) config = burnApplySFX;
		else if (lowerName.Contains("bleed")) config = bleedApplySFX;

		if (config != null)
		{
			PlaySFX(config);
		}
	}

	#endregion

	#endregion

	#region Internal helpers

	private AudioSource GetAvailableAudioSource()
	{
		// Clean up null references
		sfxPool.RemoveAll(source => source == null);

		foreach (var source in sfxPool)
		{
			if (!source.isPlaying)
			{
				return source;
			}
		}

		if (sfxPool.Count < maxPoolSize)
		{
			GameObject go = new GameObject("PooledAudioSource");
			go.transform.SetParent(this.transform);
			AudioSource newSource = go.AddComponent<AudioSource>();
			newSource.playOnAwake = false;
			sfxPool.Add(newSource);
			return newSource;
		}

		// Fallback: return the first one or the default sfxSource
		if (sfxPool.Count > 0)
		{
			return sfxPool[0];
		}
		return sfxSource;
	}

	private void ApplyVolume()
	{
		if (bgmSource != null)
			bgmSource.volume = bgmVolume;
		
		if (sfxSource != null)
			sfxSource.volume = sfxVolume;

		foreach (var source in sfxPool)
		{
			if (source != null)
			{
				source.volume = sfxVolume;
			}
		}
	}

	private void PlayBGM(AudioClip clip)
	{
		if (clip == null || bgmSource == null)
			return;

		if (bgmSource.clip == clip && bgmSource.isPlaying)
			return;

		bgmSource.clip = clip;
		bgmSource.volume = bgmVolume;
		bgmSource.loop = true;
		bgmSource.Play();
	}

	private void PlayBGMState(BGMState state)
	{
		currentBGMState = state;
		AudioClip clipToPlay = state switch
		{
			BGMState.MainMenu => mainMenuBGM,
			BGMState.CharacterCreation => characterCreationBGM,
			BGMState.Roaming => isNight ? roamingNightBGM : roamingDayBGM,
			BGMState.Battle => battleBgm,
			BGMState.Camp => campBgm,
			BGMState.Shop => shopBgm,
			_ => null
		};
		PlayBGM(clipToPlay);
	}

	#endregion

}
