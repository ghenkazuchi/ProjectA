using System.Collections.Generic;
using UnityEngine;

public static class BattleGridUtils
{
    public static GridPosition GetEntityPosition(EntityBase entity, PlayerParty playerParty, MonsterParty monsterParty)
    {
        var playerSlot = playerParty.partySlots.Find(s => s.entity == entity);
        if (playerSlot != null) return playerSlot.position;

        var monsterSlot = monsterParty.partySlots.Find(s => s.entity == entity);
        if (monsterSlot != null) return monsterSlot.position;

        return null;
    }

    public static bool IsAdjacent(GridPosition pos1, GridPosition pos2)
    {
        if (pos1 == null || pos2 == null) return false;
        int deltaX = Mathf.Abs(pos1.x - pos2.x);
        int deltaY = Mathf.Abs(pos1.y - pos2.y);
        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }

    public static bool IsHorizontal(GridPosition pos1, GridPosition pos2)
    {
        if (pos1 == null || pos2 == null) return false;
        return pos1.y == pos2.y && Mathf.Abs(pos1.x - pos2.x) == 1;
    }

    public static bool IsVertical(GridPosition pos1, GridPosition pos2)
    {
        if (pos1 == null || pos2 == null) return false;
        return pos1.x == pos2.x && Mathf.Abs(pos1.y - pos2.y) == 1;
    }

    public static bool IsWithinProtectRange(EntityBase protector, EntityBase target, ProtectRangeType range, PlayerParty playerParty, MonsterParty monsterParty)
    {
        if (range == ProtectRangeType.All) return true;
        GridPosition protectorPos = GetEntityPosition(protector, playerParty, monsterParty);
        GridPosition targetPos = GetEntityPosition(target, playerParty, monsterParty);
        if (protectorPos == null || targetPos == null) return false;

        switch (range)
        {
            case ProtectRangeType.Adjacent: return IsAdjacent(protectorPos, targetPos);
            case ProtectRangeType.Horizontal: return IsHorizontal(protectorPos, targetPos);
            case ProtectRangeType.Vertical: return IsVertical(protectorPos, targetPos);
            default: return true;
        }
    }

    public static bool IsBackRow(GridPosition pos)
    {
        return pos != null && pos.x == 1; // X=1 is the Back Row
    }

    public static bool IsTargetInBackRow(EntityBase target, PlayerParty playerParty, MonsterParty monsterParty)
    {
        var pos = GetEntityPosition(target, playerParty, monsterParty);
        return IsBackRow(pos);
    }

    /// <summary>
    /// Returns true if a unit can be single-targeted by the given attacker.
    /// Back-row units with an alive front-row column mate are protected,
    /// unless the attacker's weapon can pierce back-row (e.g. bows).
    /// </summary>
    public static bool IsTargetable(EntityBase target, PlayerParty playerParty, MonsterParty monsterParty, EntityBase attacker = null)
    {
        if (target != null && target.IsStealthed) return false;

        if (!HasAliveFrontAlly(target, playerParty, monsterParty))
            return true;

        // Attacker's weapon bypasses row protection
        if (attacker != null && attacker.weapon != null
            && attacker.weapon.WeaponBaseData != null
            && attacker.weapon.WeaponBaseData.canPierceBackRow)
            return true;

        return false;
    }

    public static BaseParty GetPartyOf(EntityBase e, PlayerParty playerParty, MonsterParty monsterParty)
    {
        if (e == null) return null;
        if (playerParty.partySlots.Exists(s => s.entity == e)) return playerParty;
        if (monsterParty.partySlots.Exists(s => s.entity == e)) return monsterParty;
        return null;
    }

    public static bool HasAliveFrontAlly(EntityBase target, PlayerParty playerParty, MonsterParty monsterParty)
    {
        var pos = GetEntityPosition(target, playerParty, monsterParty);
        if (pos == null || pos.x != 1) return false; // If not in the back row, they don't have a front ally protecting them

        var party = GetPartyOf(target, playerParty, monsterParty);
        if (party == null) return false;

        var frontPos = new GridPosition(0, pos.y); // Look at X=0 (Front Row) in the same column
        var frontEntity = party.GetEntityAtPosition(frontPos);

        return frontEntity != null && frontEntity.GetCurrentHP() > 0;
    }
    public static bool AreAllies(EntityBase a, EntityBase b, PlayerParty playerParty, MonsterParty monsterParty)
    {
        if (a == null || b == null) return false;
        bool aIsPlayer = playerParty.partySlots.Exists(s => s.entity == a);
        bool bIsPlayer = playerParty.partySlots.Exists(s => s.entity == b);

        bool aIsMonster = monsterParty.partySlots.Exists(s => s.entity == a);
        bool bIsMonster = monsterParty.partySlots.Exists(s => s.entity == b);

        return (aIsPlayer && bIsPlayer) || (aIsMonster && bIsMonster);
    }

    public static IEnumerable<EntityBase> GetAlliesOf(EntityBase unit, PlayerParty playerParty, MonsterParty monsterParty, BattleSystem battleSystem)
    {
        if (unit == null) yield break;

        if (playerParty.partySlots.Exists(s => s.entity == unit))
        {
            foreach (var s in playerParty.partySlots)
                if (s.entity != null && s.entity != unit && battleSystem.IsEntityAlivePublic(s.entity))
                    yield return s.entity;
            yield break;
        }
        if (monsterParty.partySlots.Exists(s => s.entity == unit))
        {
            foreach (var s in monsterParty.partySlots)
                if (s.entity != null && s.entity != unit && battleSystem.IsEntityAlivePublic(s.entity))
                    yield return s.entity;
        }
    }

    public static bool GetEntityWeaponType(EntityBase entity, out WeaponType weaponType)
    {
        weaponType = default;
        if (entity == null) return false;

        var w = entity.weapon;
        if (w == null) return false;

        var wb = w.WeaponBaseData;
        if (wb == null) return false;

        weaponType = wb.weaponType;
        return true;
    }
}
