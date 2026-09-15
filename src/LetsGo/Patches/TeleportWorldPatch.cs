using HarmonyLib;
using UnityEngine;

namespace LetsGo.Patches;

[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
internal static class TeleportWorldPatch
{
    private static void Prefix(TeleportWorld __instance, Player player)
    {
        if (!ModConfig.Enabled.Value || player == null)
        {
            return;
        }

        if (Player.m_localPlayer != null && player != Player.m_localPlayer)
        {
            return;
        }

        PortalFollow.CaptureFollowers(player, player.transform.position);
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
internal static class PlayerTeleportToPatch
{
    private static void Postfix(Player __instance, Vector3 pos, Quaternion rot, bool distantTeleport, bool __result)
    {
        if (!ModConfig.Enabled.Value || !__result || !distantTeleport || __instance == null)
        {
            return;
        }

        if (Player.m_localPlayer != null && __instance != Player.m_localPlayer)
        {
            return;
        }

        PortalFollow.CaptureFollowers(__instance, __instance.transform.position);
        PortalFollow.WarpPending(__instance, pos, rot);
    }
}
