using System.Collections.Generic;

public struct MonsterKillEvent : IGameEvent
{
    public MonsterCharacter Monster;
}

public struct BattleWinEvent : IGameEvent
{
    public float PartyHealthRatio;
    public int AlivePartyMemberCount;
    public int TotalPartyMemberCount;
    public BattleType BattleType;
    public int BattleItemUseCount;
    public List<MonsterCharacter> BattleMonsters;
}

public struct ShopPurchaseEvent : IGameEvent
{
    public EquipableBaseData Equipable;
}
public struct DamageDealtEvent: IGameEvent
{
    public int damageAmount;
}
public struct ChestOpenEvent : IGameEvent
{
}

public struct RecruitEvent : IGameEvent
{
    public BaseEntityData RecruitedCharacterData;
    public RecruitableCharacterTemplate RecruitedTemplate;
}

public struct InteractionEvent : IGameEvent
{
    public bool HasInteractableType;
    public InteracableType InteractableType;
    public SpawnableObject SpawnableObject;
    public string InteractionKey;
}
