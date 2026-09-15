using System;
using UnityEngine;

namespace LetsGo;

internal static class JuvenileSpecies
{
    public static bool IsEligibleJuvenile(Character character)
    {
        if (character == null || character.IsPlayer() || character.IsDead() || !character.IsTamed())
        {
            return false;
        }

        if (character.GetComponent<Growup>() != null)
        {
            return true;
        }

        string prefabName = character.name ?? string.Empty;
        string cleaned = character.GetPrefabName();
        return MatchesWolfCub(prefabName)
            || MatchesWolfCub(cleaned)
            || MatchesAsksvinHatchling(prefabName)
            || MatchesAsksvinHatchling(cleaned);
    }

    private static bool MatchesWolfCub(string name)
    {
        return !string.IsNullOrEmpty(name)
            && name.IndexOf("wolfcub", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool MatchesAsksvinHatchling(string name)
    {
        if (string.IsNullOrEmpty(name)
            || name.IndexOf("asksvin", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        return name.IndexOf("hatchling", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
