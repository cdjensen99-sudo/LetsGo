using UnityEngine;

namespace LetsGo;

internal static class FollowCommands
{
    public static Character LastHovered;

    public static TameOrder GetOrder(Character character)
    {
        ZDO zdo = character.GetZdo();
        if (zdo == null)
        {
            return TameOrder.Stay;
        }

        return (TameOrder)zdo.GetInt(ZdoFields.Order, (int)InferOrderFromAi(character));
    }

    public static bool IsFollowing(Character character, Player player = null)
    {
        if (character == null)
        {
            return false;
        }

        MonsterAI monsterAi = character.GetComponent<MonsterAI>();
        if (monsterAi != null)
        {
            GameObject follow = monsterAi.GetFollowTarget();
            if (follow == null)
            {
                return false;
            }

            if (player == null)
            {
                return follow.GetComponent<Player>() != null;
            }

            return follow == player.gameObject;
        }

        ZDO zdo = character.GetZdo();
        if (zdo == null)
        {
            return false;
        }

        ZDOID followId = zdo.GetZDOID(ZdoFields.FollowTarget);
        if (followId == ZDOID.None)
        {
            return false;
        }

        if (player == null)
        {
            return true;
        }

        ZDO playerZdo = player.GetZdo();
        return playerZdo != null && followId == playerZdo.m_uid;
    }

    public static bool IsFollowingAnyoneElse(Character character, Player self)
    {
        if (!IsFollowing(character))
        {
            return false;
        }

        return !IsFollowing(character, self);
    }

    public static TameOrder CycleOrder(Character character, Player player, bool showMessage)
    {
        TameOrder next = GetOrder(character) == TameOrder.Follow ? TameOrder.Stay : TameOrder.Follow;
        ApplyOrder(character, player, next, showMessage);
        return next;
    }

    public static void ApplyOrder(Character character, Player player, TameOrder order, bool showMessage)
    {
        if (character == null || player == null || !TameEligibility.IsEligible(character))
        {
            return;
        }

        Tameable tameable = character.GetComponent<Tameable>();
        if (tameable != null && !CompatPlugins.SkipOurCommandable)
        {
            tameable.m_commandable = true;
        }

        ZNetView nview = character.GetNview();
        LocalAuthority.TryClaim(character);

        character.GetZdo()?.Set(ZdoFields.Order, (int)order);

        MonsterAI monsterAi = character.GetComponent<MonsterAI>();
        if (monsterAi != null)
        {
            ApplyMonsterOrder(character, monsterAi, player, order);
        }
        else
        {
            ApplyAnimalOrder(character, player, order);
        }

        AnimalFollowController.EnsureAttached(character);
        JuvenileFollowController.EnsureAttached(character);

        if (showMessage)
        {
            player.Message(MessageHud.MessageType.Center, FormatOrderMessage(character, order));
        }
    }

    public static float GetFollowStopDistance(Character character)
    {
        if (character == null)
        {
            return ModConfig.MediumFollowDistance.Value;
        }

        float fromRadius = character.GetRadius() * ModConfig.FollowDistanceFactor.Value;
        return Mathf.Max(ModConfig.MediumFollowDistance.Value, fromRadius);
    }

    public static string FormatOrderMessage(Character character, TameOrder order)
    {
        string name = character.GetHoverName();
        if (order == TameOrder.Follow)
        {
            return name + " " + Localization.instance.Localize("$hud_tamefollow");
        }

        return name + " " + Localization.instance.Localize("$hud_tamestay");
    }

    private static TameOrder InferOrderFromAi(Character character)
    {
        return IsFollowing(character) ? TameOrder.Follow : TameOrder.Stay;
    }

    private static void ApplyMonsterOrder(Character character, MonsterAI monsterAi, Player player, TameOrder order)
    {
        ZNetView nview = character.GetNview();
        if (order == TameOrder.Follow)
        {
            monsterAi.ResetPatrolPoint();
            monsterAi.SetFollowTarget(player.gameObject);
            if (nview != null && nview.IsOwner())
            {
                nview.GetZDO()?.Set(ZDOVars.s_follow, player.GetPlayerName());
            }

            return;
        }

        monsterAi.SetFollowTarget(null);
        monsterAi.SetPatrolPoint();
        if (nview != null && nview.IsOwner())
        {
            nview.GetZDO()?.Set(ZDOVars.s_follow, string.Empty);
        }
    }

    private static void ApplyAnimalOrder(Character character, Player player, TameOrder order)
    {
        ZDO zdo = character.GetZdo();
        if (zdo == null)
        {
            return;
        }

        if (order == TameOrder.Follow)
        {
            zdo.Set(ZdoFields.FollowTarget, player.GetZDOID());
        }
        else
        {
            zdo.Set(ZdoFields.FollowTarget, ZDOID.None);
        }
    }
}
