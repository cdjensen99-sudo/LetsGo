using UnityEngine;

namespace LetsGo;

internal static class PackCall
{
    public static void TryCallFromLocal()
    {
        if (!ModConfig.EnablePackCall.Value)
        {
            return;
        }

        Player player = Player.m_localPlayer;
        if (player == null)
        {
            return;
        }

        PackCall.Apply(player);
    }

    public static void Apply(Player player)
    {
        if (player == null || !ModConfig.EnablePackCall.Value)
        {
            return;
        }

        int cap = Mathf.Clamp(ModConfig.PackCallMax.Value, 1, 50);
        float radius = ModConfig.PackCallRadius.Value;
        int taken = 0;

        foreach (Character character in Character.GetAllCharacters())
        {
            if (taken >= cap)
            {
                break;
            }

            if (!TameEligibility.IsEligible(character))
            {
                continue;
            }

            if (Vector3.Distance(player.transform.position, character.transform.position) > radius)
            {
                continue;
            }

            if (FollowCommands.IsFollowingAnyoneElse(character, player))
            {
                continue;
            }

            if (ModConfig.PackCallIgnorePenned.Value && PenDetector.IsPenned(character))
            {
                continue;
            }

            FollowCommands.ApplyOrder(character, player, TameOrder.Follow, false);
            taken++;
        }

        player.Message(MessageHud.MessageType.Center, taken == 0 ? "No tames answered." : "Follow " + taken);
    }
}
