using System.Collections.Generic;
using UnityEngine;

namespace LetsGo;

internal static class PortalFollow
{
    private static readonly Dictionary<long, List<ZDOID>> Pending = new Dictionary<long, List<ZDOID>>();

    public static void CaptureFollowers(Player player, Vector3 from)
    {
        if (!ModConfig.EnablePortalFollow.Value || CompatPlugins.SkipOurPortalFollow || player == null)
        {
            return;
        }

        if (Player.m_localPlayer != null && player != Player.m_localPlayer)
        {
            return;
        }

        List<ZDOID> ids = new List<ZDOID>();
        foreach (Character character in Character.GetAllCharacters())
        {
            if (!TameEligibility.IsEligible(character) || !FollowCommands.IsFollowing(character, player))
            {
                continue;
            }

            float maxDist = ModConfig.PortalFollowRadius.Value
                + FollowCommands.GetFollowStopDistance(character)
                + 4f;
            if (Vector3.Distance(from, character.transform.position) > maxDist)
            {
                continue;
            }

            CartAttachment.DetachFrom(character);
            ZDO zdo = character.GetZdo();
            if (zdo != null)
            {
                ids.Add(zdo.m_uid);
            }
        }

        if (ids.Count == 0)
        {
            return;
        }

        Pending[player.GetPlayerID()] = ids;
        LetsGoPlugin.Log.LogInfo("Portal-follow captured " + ids.Count + " tames for " + player.GetPlayerName());
    }

    public static void WarpPending(Player player, Vector3 dest, Quaternion rot)
    {
        if (player == null || CompatPlugins.SkipOurPortalFollow)
        {
            if (player != null)
            {
                Pending.Remove(player.GetPlayerID());
            }

            return;
        }

        if (!Pending.TryGetValue(player.GetPlayerID(), out List<ZDOID> ids))
        {
            return;
        }

        Pending.Remove(player.GetPlayerID());
        Vector3 back = rot * Vector3.back;
        int i = 0;
        foreach (ZDOID id in ids)
        {
            Vector3 offset = dest + (back * (2f + (i * 1.2f))) + Vector3.up;
            PlaceAt(id, offset, rot);
            i++;
        }
    }

    private static void PlaceAt(ZDOID id, Vector3 position, Quaternion rotation)
    {
        Character character = TameEligibility.ResolveCharacter(id);
        if (character != null)
        {
            ZNetView nview = character.GetNview();
            if (nview != null && nview.IsValid() && !nview.IsOwner())
            {
                nview.ClaimOwnership();
            }

            character.transform.position = position;
            character.transform.rotation = rotation;
            character.GetZdo()?.SetPosition(position);
            character.GetZdo()?.SetRotation(rotation);
            return;
        }

        ZDO zdo = ZDOMan.instance?.GetZDO(id);
        if (zdo == null || !zdo.IsValid())
        {
            return;
        }

        zdo.SetPosition(position);
        zdo.SetRotation(rotation);
    }
}
