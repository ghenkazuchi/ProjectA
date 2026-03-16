using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HaKien
{
    public enum MessageType
    {
		OnCharacterCreationEnter,
		OnGameStart,
        OnGameLose,
        OnGameWin,
        OnButtonClick,
        OnDataChanged,
        OnTimeChanged,
        OnInitUI,
        OnStatusChange,
        OnDataChangedDuringTurn,
        OnSelectDifficulty,
        OnSkillActive,
        OnGameRestart,
        OnBattleStart,
        OnTargetSelection,
        OnBattleOver,
        OnRecruitEnter,
        OnRecruitCharacter,
        OnRecruitCharacterUIClose,
        OnPartyMenuOpen,
        OnPartyMenuClose,
		OnPartyMemberInfoUpdate,
        OnPartyMemberInfoClose,
        OnChestOpen,
        OnItemSelected,
        OnChestClose,
		OnInventoryOpen,
		OnInventoryItemSelected,
		OnInventoryClose,  
        OnInteract,
        OnInteractEnd,
        //popup
        OnFireCampPopupOpen,
		OnFireCampPopupClose,
		OnAnvilPopupOpen,
		OnAnvilPopupClose,
        OnSelectCharacterReturn,
		OnShopOpen,
		//Shop Specific
        OnShopItemSelected,
        OnShopClose,
        OnShopPurchaceRequest,
        OnShopPurchaceSucceeded,
		OnShopPurchaceFailed,
		OnShopRerollRequest,
        OnShopUpdated,
        OnShopCharacterSelectionOpen,
		OnShopCharacterSelectionClose,
        OnShopCharacterInventoryOpen,
		OnShopCharacterInventoryClose,  
        OnShopCharacterSelected,
		OnShopCharacterReplaceSelectionConfirmed,
        OnSelectedEquipableConclude,
        //Hero spirit 
        OnHeroSpiritInteractionPopupOpen,
		OnHeroSpiritInteractionPopupClose,
		OnAchievementScreenOpen,
		OnAchievementScreenClose,

	}
    public class Message
    {
        public MessageType type;
        public object[] data;

        public Message(MessageType type)
        {
            this.type = type;
        }

        public Message(MessageType type, object[] data)
        {
            this.type = type;
            this.data = data;
        }
    }
    public interface IMessageHandle
    {
        void Handle(Message message);
    }
    public class MessageManager : Singleton<MessageManager>, ISerializationCallbackReceiver
    {
        // private static MessageManager instance = null;

        //Stores information when Serialize data in the subcribers-Dictionary
        [HideInInspector] public List<MessageType> _keys = new List<MessageType>();
        [HideInInspector] public List<List<IMessageHandle>> _values = new List<List<IMessageHandle>>();


        private Dictionary<MessageType, List<IMessageHandle>> subcribers = new Dictionary<MessageType, List<IMessageHandle>>();
        /*public static MessageManager Instance { get { return instance; } }
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }*/
        public void AddSubcriber(MessageType type, IMessageHandle handle)
        {
            if (!subcribers.ContainsKey(type))
                subcribers[type] = new List<IMessageHandle>();
            if (!subcribers[type].Contains(handle))
                subcribers[type].Add(handle);
        }

        public void RemoveSubcriber(MessageType type, IMessageHandle handle)
        {
            if (subcribers.ContainsKey(type))
                if (subcribers[type].Contains(handle))
                    subcribers[type].Remove(handle);
        }

        public void SendMessage(Message message)
        {
            if (subcribers.ContainsKey(message.type))
                for (int i = subcribers[message.type].Count - 1; i > -1; i--)
                    subcribers[message.type][i].Handle(message);
        }

        public void SendMessageWithDelay(Message message, float delay)
        {
            StartCoroutine(_DelaySendMessage(message, delay));
        }

        private IEnumerator _DelaySendMessage(Message message, float delay)
        {
            yield return new WaitForSeconds(delay);
            SendMessage(message);
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var element in subcribers)
            {
                _keys.Add(element.Key);
                _values.Add(element.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            subcribers = new Dictionary<MessageType, List<IMessageHandle>>();
            for (int i = 0; i < _keys.Count; i++)
            {
                subcribers.Add(_keys[i], _values[i]);
            }
        }
    }
}
