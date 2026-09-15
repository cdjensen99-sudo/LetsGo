using HarmonyLib;
using UnityEngine;

namespace LetsGo.Patches;

[HarmonyPatch(typeof(Game), "Start")]
internal static class GameStartPatch
{
    private static void Postfix()
    {
        LetsGoRpc.Register();
    }
}

[HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
[HarmonyPriority(Priority.First)]
internal static class TameableInteractPatch
{
    private static bool Prefix(Tameable __instance, Humanoid user, bool hold, bool alt, ref bool __result)
    {
        Character character = __instance.GetComponent<Character>();
        if (JuvenileSpecies.IsEligibleJuvenile(character) && character.IsTamed())
        {
            if (!hold && ModConfig.EnableFollow.Value)
            {
                __instance.m_commandable = true;
            }

            return true;
        }

        Player player = user as Player;
        if (!TameInteractHelper.TryHandle(player, character, hold, alt))
        {
            return true;
        }

        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
internal static class TameableHoverPatch
{
    private static void Postfix(Tameable __instance, ref string __result)
    {
        Character character = __instance.GetComponent<Character>();
        if (JuvenileSpecies.IsEligibleJuvenile(character))
        {
            JuvenileFollowDisplay.ApplyFollowHover(character, ref __result);
            return;
        }

        AppendFollowHover(character, ref __result);
    }

    internal static void AppendFollowHover(Character character, ref string __result)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableFollow.Value || !TameEligibility.IsEligible(character))
        {
            return;
        }

        FollowCommands.LastHovered = character;
        if (string.IsNullOrEmpty(__result))
        {
            __result = character.GetHoverName();
        }

        TameOrder order = FollowCommands.GetOrder(character);
        string commandKey = order == TameOrder.Follow ? "$hud_tamestay" : "$hud_tamefollow";
        string localizedPet = Localization.instance.Localize("$hud_pet");
        string localizedCommand = Localization.instance.Localize(commandKey);
        if (!string.IsNullOrEmpty(__result) && __result.Contains(localizedPet))
        {
            __result = __result.Replace(localizedPet, localizedCommand);
        }
        else if (string.IsNullOrEmpty(__result) || !__result.Contains(localizedCommand))
        {
            __result += Localization.instance.Localize(
                $"\n[<color=yellow><b>$KEY_Use</b></color>] {commandKey}");
        }
    }
}

[HarmonyPatch(typeof(AnimalAI), nameof(AnimalAI.UpdateAI))]
internal static class AnimalAIFollowPatch
{
    private static bool Prefix(AnimalAI __instance, float dt, ref bool __result)
    {
        if (JuvenileFollowController.TryApplyAnimalFollow(__instance, dt)
            || AnimalFollowController.TryApply(__instance, dt))
        {
            __result = true;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(BaseAI), "Follow")]
internal static class BaseAIFollowPatch
{
    private static bool Prefix(BaseAI __instance, GameObject go, float dt)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableFollow.Value)
        {
            return true;
        }

        Character character = BaseAIAccess.GetCharacter(__instance);
        if (character == null
            || (!TameEligibility.IsEligible(character) && !JuvenileSpecies.IsEligibleJuvenile(character)))
        {
            return true;
        }

        if (go == null || go.GetComponent<Player>() == null)
        {
            return true;
        }

        float distance = Vector3.Distance(go.transform.position, __instance.transform.position);
        float followDistance = FollowCommands.GetFollowStopDistance(character);
        bool run = distance > followDistance * 3f;
        if (distance < followDistance)
        {
            __instance.StopMoving();
        }
        else
        {
            BaseAIAccess.MoveTo(__instance, dt, go.transform.position, 0f, run);
        }

        return false;
    }
}

[HarmonyPatch(typeof(Player), "FindHoverObject")]
[HarmonyPriority(900)]
internal static class PlayerFindHoverObjectPatch
{
    private static void Postfix(Player __instance, ref GameObject hover, ref Character hoverCreature)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableFollow.Value)
        {
            return;
        }

        Character juvenile = JuvenileInteractHelper.FindJuvenileUnderCrosshair(
            __instance, __instance.m_maxInteractDistance);
        if (juvenile != null)
        {
            hover = juvenile.gameObject;
            hoverCreature = juvenile;
            FollowCommands.LastHovered = juvenile;
            return;
        }

        Character assist = TameInteractHelper.FindAssistTarget(__instance, __instance.m_maxInteractDistance);
        if (assist == null)
        {
            return;
        }

        hover = assist.gameObject;
        hoverCreature = assist;
        FollowCommands.LastHovered = assist;
    }
}

[HarmonyPatch(typeof(Player), "Interact")]
[HarmonyPriority(900)]
internal static class PlayerTameInteractPatch
{
    private static bool Prefix(Player __instance, GameObject go, bool hold, bool alt)
    {
        if (!ModConfig.Enabled.Value || hold)
        {
            return true;
        }

        if (!alt)
        {
            Character juvenile = JuvenileInteractHelper.FindJuvenileUnderCrosshair(__instance)
                ?? __instance.GetHoverCreature();
            if (juvenile != null
                && juvenile.IsTamed()
                && JuvenileSpecies.IsEligibleJuvenile(juvenile)
                && ModConfig.EnableFollow.Value)
            {
                JuvenileGroundSnapper.EnsureAttached(juvenile);
                JuvenileFollowController.EnsureAttached(juvenile);
                if (JuvenileFollow.TryCommand(juvenile, __instance, showMessage: true))
                {
                    return false;
                }
            }
        }

        Character creature = TameInteractHelper.FindAssistTarget(__instance)
            ?? __instance.GetHoverCreature();
        if (creature == null
            || JuvenileSpecies.IsEligibleJuvenile(creature)
            || !TameEligibility.NeedsCrosshairAssist(creature))
        {
            return true;
        }

        return !TameInteractHelper.TryHandle(__instance, creature, hold, alt);
    }
}

[HarmonyPatch(typeof(Character), "GetHoverText")]
internal static class CharacterTameHoverPatch
{
    private static void Postfix(Character __instance, ref string __result)
    {
        JuvenileFollowDisplay.ApplyFollowHover(__instance, ref __result);

        if (!ModConfig.Enabled.Value || !TameEligibility.NeedsCrosshairAssist(__instance))
        {
            return;
        }

        if (JuvenileSpecies.IsEligibleJuvenile(__instance))
        {
            return;
        }

        if (__instance.GetComponent<Tameable>() != null && !string.IsNullOrEmpty(__result) && __result.Contains("$KEY_Use"))
        {
            return;
        }

        string hover = string.Empty;
        TameableHoverPatch.AppendFollowHover(__instance, ref hover);
        if (!string.IsNullOrEmpty(hover))
        {
            __result = string.IsNullOrEmpty(__result) ? hover : __result + hover;
        }
    }
}

[HarmonyPatch(typeof(Character), "Awake")]
internal static class CharacterAwakePatch
{
    private static void Postfix(Character __instance)
    {
        JuvenileGroundSnapper.EnsureAttached(__instance);
        JuvenileFollowController.EnsureAttached(__instance);

        if (!TameEligibility.IsEligible(__instance))
        {
            return;
        }

        AnimalFollowController.EnsureAttached(__instance);
    }
}
