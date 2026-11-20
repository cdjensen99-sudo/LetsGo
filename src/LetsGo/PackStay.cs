using UnityEngine;

namespace LetsGo;

internal static class PackStay
{
    public static void TryStayFromLocal()
    {
        if (!ModConfig.EnablePackStay.Value)
        {
            return;
        }

        Player player = Player.m_localPlayer;
        if (player == null)
        {
            return;
        }

        Apply(player);
    }

    public static void Apply(Player player)
    {
        if (player == null || !ModConfig.EnablePackStay.Value)
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

            if (!FollowCommands.IsFollowing(character, player))
            {
                continue;
            }

            FollowCommands.ApplyOrder(character, player, TameOrder.Stay, false);
            taken++;
        }

        player.Message(MessageHud.MessageType.Center, taken == 0 ? "No tames stayed." : "Stay " + taken);
    }
}
