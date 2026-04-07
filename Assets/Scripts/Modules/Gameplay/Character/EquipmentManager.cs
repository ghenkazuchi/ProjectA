using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentManager
{
    private PlayerCharacter player;
    private readonly SetBonusManager setTracker = new SetBonusManager();
    private object setSourceKey;

    public EquipmentManager(PlayerCharacter owner)
    {
        this.player = owner;
    }

    private void EnsureSetSource()
    {
        if (setSourceKey == null)
            setSourceKey = new object();
    }

    public void RefreshSetBonuses()
    {
        setTracker.Recalculate(player.items, player.weapon);
        EnsureSetSource();

        player.EquipmentEffectRunner.UnregisterEffectBinding(setSourceKey);
        var active = setTracker.GetAllActiveBindings();
        if (active.Count > 0)
        {
            player.EquipmentEffectRunner.RegisterEffectBinding(setSourceKey, active);
        }
        RebuildPassiveEffects();
    }

    public void RebuildPassiveEffects()
    {
        foreach (var oldEffect in player.passiveEquipmentEffects)
        {
            if (oldEffect != null)
            {
                var removeCoroutine = oldEffect.RemoveEffect();
                while (removeCoroutine.MoveNext()) { }
            }
        }
        player.passiveEquipmentEffects.Clear();

        if (player.weapon != null && player.weapon.WeaponBaseData != null)
            ExtractPassives(player.weapon.WeaponBaseData.effectData);

        foreach (var item in player.items)
            if (item != null && item.itemBaseData != null)
                ExtractPassives(item.itemBaseData.effectData);

        ExtractPassives(setTracker.GetAllActiveBindings());
        
        player.CalculateAllStats();
    }

    private void ExtractPassives(List<EquipEffectBinding> bindings)
    {
        if (bindings == null) return;
        foreach (var binding in bindings)
        {
            if (binding.trigger == EquipEffectTrigger.OnEquip && binding.effect != null && binding.effect.isPassiveEquipmentEffect)
            {
                var effect = binding.effect.CreateRuntimeEffect(player, player, binding.effect.MaxDuration);
                player.passiveEquipmentEffects.Add(effect);
                var applyCoroutine = effect.ApplyEffect();
                while (applyCoroutine.MoveNext()) { }
            }
        }
    }

    public bool TryEquipWeapon(Weapon weaponToEquip)
    {
        if (weaponToEquip == null || !player.GetClassData.usableWeaponTypes.Contains(weaponToEquip.WeaponBaseData.weaponType))
        {
            Debug.Log("Weapon type not usable by this class or weapon is null.");
            return false;
        }
        int requiredSlot = weaponToEquip.WeaponBaseData.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
        int availableSlot = player.GetClassData.itemSlotCount - player.items.Count;
        if (availableSlot < requiredSlot)
        {
            Debug.Log("Don't have enough slot");
            return false;
        }
        
        UnequipWeapon();
        
        player.weapon = weaponToEquip;
        player.storedEquipmentBindings.AddRange(player.weapon.WeaponBaseData.effectData);

        for (int i = 0; i < requiredSlot; i++)
        {
            player.items.Add(null);
        }
        player.EquipmentEffectRunner.RegisterEffectBinding(weaponToEquip, weaponToEquip.WeaponBaseData.effectData);
        RefreshSetBonuses();
        Debug.Log("Equipped weapon: " + weaponToEquip.WeaponBaseData.name);
        return true;
    }

    public void UnequipWeapon()
    {
        if (player.weapon == null) return;
        if (player.weapon.WeaponBaseData != null)
        {
            player.EquipmentEffectRunner.UnregisterEffectBinding(player.weapon);
            foreach (var b in player.weapon.WeaponBaseData.effectData)
                player.storedEquipmentBindings.Remove(b);
        }

        int slotToFree = player.weapon.WeaponBaseData != null && player.weapon.WeaponBaseData.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
        player.weapon = null;

        int removed = 0;
        for (int i = player.items.Count - 1; i >= 0 && removed < slotToFree; i--)
        {
            if (player.items[i] == null)
            {
                player.items.RemoveAt(i);
                removed++;
            }
        }
        RefreshSetBonuses();
        Debug.Log("Unequipped weapon.");
    }

    public bool TryAddItem(Item item)
    {
        if (item == null)
            return false;
        if (player.items.Count >= player.GetClassData.itemSlotCount)
        {
            Debug.Log("Not enough slots to add item: " + item.itemBaseData.itemName);
            return false;
        }
        bool hasSameBase = player.items.Any(it => it != null && it.itemBaseData == item.itemBaseData);
        player.items.Add(item);
        if (hasSameBase && item.itemBaseData.canDuplicateTrigger == false)
        {
            return true;
        }
        player.storedEquipmentBindings.AddRange(item.itemBaseData.effectData);
        player.EquipmentEffectRunner.RegisterEffectBinding(item, item.itemBaseData.effectData);
        RefreshSetBonuses();
        Debug.Log("Added item: " + item.itemBaseData.itemName);
        return true;
    }

    public Item RemoveItemAtSlot(int index)
    {
        if (index < 0 || index >= player.items.Count) return null;

        var oldItem = player.items[index];
        if (oldItem != null)
        {
            player.EquipmentEffectRunner.UnregisterEffectBinding(oldItem);
            foreach (var b in oldItem.itemBaseData.effectData)
                player.storedEquipmentBindings.Remove(b);
        }
        if (oldItem != null && oldItem.itemBaseData.canDuplicateTrigger == true)
        {
            var replacement = player.items.FirstOrDefault(it => it != null && it != oldItem && it.itemBaseData == oldItem.itemBaseData);
            if (replacement != null)
            {
                player.EquipmentEffectRunner.RegisterEffectBinding(replacement, replacement.itemBaseData.effectData);
            }
        }
        player.items.RemoveAt(index);
        RefreshSetBonuses();
        Debug.Log("Removed item at slot: " + index);
        return oldItem;
    }

    public Item GetItemAtSlot(int index)
    {
        if (index < 0 || index >= player.items.Count) return null;
        return player.items[index];
    }

    public string GetItemSlotStatus()
    {
        int usedSlots = player.items.Count;
        int totalSlots = player.GetClassData.itemSlotCount;
        return $"Slots: {usedSlots}/{totalSlots}";
    }

    public int GetTotalSlots()
    {
        return player.GetClassData.itemSlotCount;
    }

    public int GetUsedSlots()
    {
        return player.items.Count;
    }

    public int GetFreeSlots()
    {
        return GetTotalSlots() - GetUsedSlots();
    }

    public int GetSlotCostForEquipable(EquipableBaseData data)
    {
        if (data is WeaponBaseData w)
        {
            return w.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
        }

        return 1;
    }

    public void ApplyReplaceSelection(bool removeWeapon, List<int> removeItemIndices)
    {
        if (removeWeapon && player.weapon != null)
        {
            UnequipWeapon();
        }

        if (removeItemIndices != null && removeItemIndices.Count > 0)
        {
            removeItemIndices.Sort();
            for (int i = removeItemIndices.Count - 1; i >= 0; i--)
            {
                int idx = removeItemIndices[i];
                RemoveItemAtSlot(idx);
            }
        }
    }

    public string GetWeaponStatus()
    {
        if (player.weapon != null && player.weapon.WeaponBaseData != null)
        {
            return player.weapon.WeaponBaseData.itemName;
        }
        else
        {
            return "No weapon";
        }
    }

    public void OnBattleStartSyncSet() => RefreshSetBonuses();
}
