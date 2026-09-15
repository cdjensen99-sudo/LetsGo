using System;
using UnityEngine;

namespace LetsGo;

internal static class TameEligibility
{
    public static bool IsEligible(Character character)
    {
        if (character == null || character.IsPlayer() || character.IsDead())
        {
            return false;
        }

        if (!character.IsTamed())
        {
            return false;
        }

        if (character.GetComponent<Tameable>() == null && !JuvenileSpecies.IsEligibleJuvenile(character))
        {
            return false;
        }

        if (IsExcluded(character))
        {
            return false;
        }

        return character.GetComponent<MonsterAI>() != null || character.GetComponent<AnimalAI>() != null;
    }

    public static bool NeedsCrosshairAssist(Character character)
    {
        if (!IsEligible(character))
        {
            return false;
        }

        if (character.GetComponent<Growup>() != null || character.GetComponent<AnimalAI>() != null)
        {
            return true;
        }

        return IsWolfCubByName(character);
    }

    public static bool IsWolfCubByName(Character character)
    {
        string prefab = character.GetPrefabName();
        return !string.IsNullOrEmpty(prefab)
            && prefab.IndexOf("wolfcub", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool IsExcluded(Character character)
    {
        string prefab = character.GetPrefabName();
        if (string.IsNullOrEmpty(prefab))
        {
            return false;
        }

        string list = ModConfig.ExcludePrefabs?.Value;
        if (string.IsNullOrWhiteSpace(list))
        {
            return false;
        }

        foreach (string raw in list.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (prefab.Equals(raw.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static Player ResolvePlayer(ZDOID playerId)
    {
        if (playerId == ZDOID.None)
        {
            return null;
        }

        GameObject instance = ZNetScene.instance?.FindInstance(playerId);
        Player player = instance != null ? instance.GetComponent<Player>() : null;
        if (player != null)
        {
            return player;
        }

        foreach (Player online in Player.GetAllPlayers())
        {
            ZDO zdo = online.GetZdo();
            if (zdo != null && zdo.m_uid == playerId)
            {
                return online;
            }
        }

        return null;
    }

    public static Character ResolveCharacter(ZDOID id)
    {
        GameObject instance = ZNetScene.instance?.FindInstance(id);
        return instance != null ? instance.GetComponent<Character>() : null;
    }
}
