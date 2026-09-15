using UnityEngine;

namespace LetsGo;

internal static class LetsGoRpc
{
    private const string RpcOrder = "hw_letsgo_order";
    private const string RpcExecuteOrder = "hw_letsgo_order_exec";
    private const string RpcPackCall = "hw_letsgo_pack";
    private const string RpcExecutePackCall = "hw_letsgo_pack_exec";

    public static void Register()
    {
        ZRoutedRpc.instance.Register<ZDOID, ZDOID, int>(RpcOrder, OnOrder);
        ZRoutedRpc.instance.Register<ZDOID, ZDOID, int>(RpcExecuteOrder, OnExecuteOrder);
        ZRoutedRpc.instance.Register<ZDOID>(RpcPackCall, OnPackCall);
        ZRoutedRpc.instance.Register<ZDOID>(RpcExecutePackCall, OnExecutePackCall);
        JuvenileFollowRpc.Register();
    }

    public static void RequestCycleOrder(Character character, Player player)
    {
        RequestOrder(character, player, 0, () => FollowCommands.CycleOrder(character, player, true));
    }

    public static void RequestFollow(Character character, Player player)
    {
        RequestOrder(
            character,
            player,
            1,
            () => FollowCommands.ApplyOrder(character, player, TameOrder.Follow, true));
    }

    public static void RequestPackCall(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (Player.m_localPlayer == player || (ZNet.instance != null && ZNet.instance.IsServer()))
        {
            PackCall.Apply(player);
            return;
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(RpcPackCall, player.GetZDOID());
    }

    private static void RequestOrder(Character character, Player player, int mode, System.Action local)
    {
        ZDO zdo = character?.GetZdo();
        if (zdo == null || player == null)
        {
            return;
        }

        if (LocalAuthority.HasLoaded(character))
        {
            local();
            return;
        }

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            return;
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(RpcOrder, zdo.m_uid, player.GetZDOID(), mode);
    }

    private static void OnOrder(long sender, ZDOID creatureId, ZDOID playerId, int mode)
    {
        if (!ZNet.instance.IsServer())
        {
            return;
        }

        if (TryApplyOrder(creatureId, playerId, sender, mode))
        {
            return;
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcExecuteOrder, creatureId, playerId, mode);
    }

    private static void OnExecuteOrder(long sender, ZDOID creatureId, ZDOID playerId, int mode)
    {
        TryApplyOrder(creatureId, playerId, sender, mode);
    }

    private static void OnPackCall(long sender, ZDOID playerId)
    {
        if (!ZNet.instance.IsServer())
        {
            return;
        }

        if (TryApplyPackCall(playerId))
        {
            return;
        }

        ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcExecutePackCall, playerId);
    }

    private static void OnExecutePackCall(long sender, ZDOID playerId)
    {
        TryApplyPackCall(playerId);
    }

    private static bool TryApplyOrder(ZDOID creatureId, ZDOID playerId, long sender, int mode)
    {
        Character creature = TameEligibility.ResolveCharacter(creatureId);
        Player player = TameEligibility.ResolvePlayer(playerId) ?? Player.GetPlayer(sender) ?? Player.m_localPlayer;
        if (creature == null || player == null)
        {
            return false;
        }

        if (mode == 1)
        {
            FollowCommands.ApplyOrder(creature, player, TameOrder.Follow, true);
        }
        else
        {
            FollowCommands.CycleOrder(creature, player, true);
        }

        return true;
    }

    private static bool TryApplyPackCall(ZDOID playerId)
    {
        Player player = TameEligibility.ResolvePlayer(playerId) ?? Player.m_localPlayer;
        if (player == null)
        {
            return false;
        }

        PackCall.Apply(player);
        return true;
    }
}
