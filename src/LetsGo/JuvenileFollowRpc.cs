using UnityEngine;

namespace LetsGo;

internal static class JuvenileFollowRpc
{
    private const string RpcToggleFollow = "hw_letsgo_juv_follow";
    private const string RpcExecuteToggleFollow = "hw_letsgo_juv_follow_exec";

    public static void Register()
    {
        ZRoutedRpc.instance.Register<ZDOID, ZDOID, bool>(RpcToggleFollow, OnToggleFollowRpc);
        ZRoutedRpc.instance.Register<ZDOID, ZDOID, bool>(RpcExecuteToggleFollow, OnExecuteToggleFollowRpc);
    }

    public static void RequestToggle(ZDOID creatureId, ZDOID playerId, bool showMessage)
    {
        ZRoutedRpc.instance.InvokeRoutedRPC(RpcToggleFollow, creatureId, playerId, showMessage);
    }

    private static void OnToggleFollowRpc(long sender, ZDOID creatureId, ZDOID playerId, bool showMessage)
    {
        if (!ZNet.instance.IsServer())
        {
            return;
        }

        if (TryApply(creatureId, playerId, sender, showMessage))
        {
            return;
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcExecuteToggleFollow, creatureId, playerId, showMessage);
    }

    private static void OnExecuteToggleFollowRpc(long sender, ZDOID creatureId, ZDOID playerId, bool showMessage)
    {
        TryApply(creatureId, playerId, sender, showMessage);
    }

    private static bool TryApply(ZDOID creatureId, ZDOID playerId, long sender, bool showMessage)
    {
        Character creature = TameEligibility.ResolveCharacter(creatureId);
        Player player = ResolveRequestingPlayer(sender, playerId);
        if (creature == null || player == null)
        {
            return false;
        }

        if (!JuvenileSpecies.IsEligibleJuvenile(creature) || !creature.IsTamed())
        {
            return false;
        }

        JuvenileGroundSnapper.EnsureAttached(creature);
        JuvenileFollowController.EnsureAttached(creature);
        JuvenileFollow.ApplyFollowToggle(creature, player, showMessage);
        return true;
    }

    private static Player ResolveRequestingPlayer(long sender, ZDOID playerId)
    {
        Player senderPlayer = Player.GetPlayer(sender);
        if (senderPlayer != null)
        {
            return senderPlayer;
        }

        Player resolved = TameEligibility.ResolvePlayer(playerId);
        if (resolved != null)
        {
            return resolved;
        }

        return Player.m_localPlayer;
    }
}
