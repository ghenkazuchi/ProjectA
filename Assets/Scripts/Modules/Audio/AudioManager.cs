using System.Collections;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>, IMessageHandle
{
	[Header("BGM")]
	[SerializeField] private AudioSource bgmSource;
	[SerializeField] private AudioClip roamingBGM;
	[SerializeField] private AudioClip characterCreationBGM;
	[SerializeField] private AudioClip battleBgm;
	[SerializeField] private AudioClip campBgm;
	[SerializeField] private AudioClip shopBgm;

	[Header("SFX")]
	[SerializeField] private AudioSource sfxSource;
	[SerializeField] private AudioClip buttonClickSfx;
	[SerializeField] private AudioClip chestOpenSfx;
	[SerializeField] private AudioClip itemGetSfx;
	[SerializeField] private AudioClip shopOpenSfx;
	[SerializeField] private AudioClip shopCloseSfx;
	[SerializeField] private AudioClip battleStartSfx;
	[SerializeField] private AudioClip skillCastSfx;
	[SerializeField] private AudioClip gameWinSfx;
	[SerializeField] private AudioClip gameLoseSfx;

	[Header("Volume")]
	[Range(0f, 1f)] public float bgmVolume = 0.6f;
	[Range(0f, 1f)] public float sfxVolume = 1f;

	#region Unity lifecycle

	private void Awake()
	{
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
		mm.AddSubcriber(MessageType.OnInteractEnd,this);
		mm.AddSubcriber(MessageType.OnFireCampPopupOpen, this);
	}

	private void OnDisable()
	{
		var mm = MessageManager.Instance;

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
	}

	#endregion

	#region IMessageHandle

	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnCharacterCreationEnter:
				PlayBGM(characterCreationBGM);
				break;
			case MessageType.OnGameStart:
				PlayBGM(roamingBGM);
				break;

			case MessageType.OnBattleStart:
				PlayBGM(battleBgm);
				PlaySFX(battleStartSfx);
				break;

			case MessageType.OnBattleOver:
				PlayBGM(campBgm);
				break;

			case MessageType.OnGameWin:
				PlaySFX(gameWinSfx);
				break;

			case MessageType.OnGameLose:
				PlaySFX(gameLoseSfx);
				break;

			case MessageType.OnButtonClick:
				PlaySFX(buttonClickSfx);
				break;

			case MessageType.OnChestOpen:
				PlaySFX(chestOpenSfx);
				break;

			case MessageType.OnShopOpen:
				PlaySFX(shopOpenSfx);
				PlayBGM(shopBgm);
				break;

			case MessageType.OnShopClose:
				PlaySFX(shopCloseSfx);
				PlayBGM(campBgm);
				break;

			case MessageType.OnSkillActive:
				PlaySFX(skillCastSfx);
				break;

			case MessageType.OnInteract:
				PlaySFX(itemGetSfx);
				break;
			case MessageType.OnInteractEnd:
				PlayBGM(roamingBGM);
				break;
			case MessageType.OnFireCampPopupOpen:
				PlayBGM(campBgm);
				break;
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
		if (sfxSource != null)
			sfxSource.volume = sfxVolume;
	}

	#endregion

	#region Internal helpers

	private void ApplyVolume()
	{
		if (bgmSource != null)
			bgmSource.volume = bgmVolume;
		if (sfxSource != null)
			sfxSource.volume = sfxVolume;
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

	private void PlaySFX(AudioClip clip)
	{
		if (clip == null || sfxSource == null)
			return;

		sfxSource.volume = sfxVolume;
		sfxSource.PlayOneShot(clip);
	}

	#endregion
}
