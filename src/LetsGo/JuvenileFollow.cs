using UnityEngine;

namespace LetsGo;

internal static class JuvenileFollow
{
    public static bool TryCommand(Character character, Player player, bool showMessage)
    {
        if (character == null || player == null)
        {
            return false;
        }

        if (!JuvenileSpecies.IsEligibleJuvenile(character) || !character.IsTamed())
        {
            return false;
        }

        ZNetView nview = character.GetNview();
        if (nview == null || !nview.IsValid())
        {
            return false;
        }

        JuvenileGroundSnapper.EnsureAttached(character);
        JuvenileFollowController.EnsureAttached(character);

        if (LocalAuthority.HasLoaded(character))
        {
            return ApplyFollowToggle(character, player, showMessage);
        }

        JuvenileFollowRpc.RequestToggle(nview.GetZDO().m_uid, player.GetZDOID(), showMessage);
        return true;
    }

    public static bool ApplyFollowToggle(Character character, Player player, bool showMessage)
    {
        if (character == null || player == null || !character.IsTamed())
        {
            return false;
        }

        MonsterAI monsterAi = character.GetComponent<MonsterAI>();
        if (monsterAi != null)
        {
            Tameable tameable = character.GetComponent<Tameable>();
            if (tameable != null)
            {
                tameable.m_commandable = true;
            }

            ApplyMonsterFollowToggle(character, monsterAi, player, showMessage);
            SyncOrderZdo(character);
            return true;
        }

        if (character.GetComponent<AnimalAI>() != null)
        {
            bool applied = ApplyAnimalFollowToggle(character, player, showMessage);
            if (applied)
            {
                SyncOrderZdo(character);
            }

            return applied;
        }

        return false;
    }

    public static bool IsFollowing(Character character)
    {
        if (character == null)
        {
            return false;
        }

        MonsterAI monsterAi = character.GetComponent<MonsterAI>();
        if (monsterAi != null && monsterAi.GetFollowTarget() != null)
        {
            return true;
        }

        ZDO zdo = character.GetZdo();
        return zdo != null && zdo.GetZDOID(ZdoFields.FollowTarget) != ZDOID.None;
    }

    private static void SyncOrderZdo(Character character)
    {
        character.GetZdo()?.Set(
            ZdoFields.Order,
            IsFollowing(character) ? (int)TameOrder.Follow : (int)TameOrder.Stay);
    }

    private static void ApplyMonsterFollowToggle(
        Character character,
        MonsterAI monsterAi,
        Player player,
        bool showMessage)
    {
        ZNetView nview = character.GetNview();
        if (nview != null && !nview.IsOwner())
        {
            nview.ClaimOwnership();
        }

        string displayName = character.GetHoverName();
        if (monsterAi.GetFollowTarget() != null)
        {
            monsterAi.SetFollowTarget(null);
            monsterAi.SetPatrolPoint();
            if (nview != null && nview.IsOwner())
            {
                nview.GetZDO()?.Set(ZDOVars.s_follow, string.Empty);
                nview.GetZDO()?.Set(ZdoFields.FollowTarget, ZDOID.None);
            }

            if (showMessage)
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    displayName + " " + Localization.instance.Localize("$hud_tamestay"));
            }

            return;
        }

        monsterAi.ResetPatrolPoint();
        monsterAi.SetFollowTarget(player.gameObject);
        if (nview != null && nview.IsOwner())
        {
            nview.GetZDO()?.Set(ZDOVars.s_follow, player.GetPlayerName());
            nview.GetZDO()?.Set(ZdoFields.FollowTarget, player.GetZDOID());
        }

        if (showMessage)
        {
            player.Message(
                MessageHud.MessageType.Center,
                displayName + " " + Localization.instance.Localize("$hud_tamefollow"));
        }
    }

    public static bool ApplyAnimalFollowToggle(Character character, Player player, bool showMessage)
    {
        ZNetView nview = character.GetNview();
        if (nview == null || !nview.IsValid())
        {
            return false;
        }

        if (!nview.IsOwner())
        {
            nview.ClaimOwnership();
        }

        ZDO zdo = character.GetZdo();
        if (zdo == null)
        {
            return false;
        }

        ZDOID playerId = player.GetZDOID();
        ZDOID currentFollow = zdo.GetZDOID(ZdoFields.FollowTarget);
        string displayName = character.GetHoverName();

        if (currentFollow == playerId)
        {
            zdo.Set(ZdoFields.FollowTarget, ZDOID.None);
            if (showMessage)
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    displayName + " " + Localization.instance.Localize("$hud_tamestay"));
            }
        }
        else
        {
            zdo.Set(ZdoFields.FollowTarget, playerId);
            if (showMessage)
            {
                player.Message(
                    MessageHud.MessageType.Center,
                    displayName + " " + Localization.instance.Localize("$hud_tamefollow"));
            }
        }

        return true;
    }
}
