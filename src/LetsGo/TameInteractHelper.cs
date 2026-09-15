using System.Linq;
using UnityEngine;

namespace LetsGo;

internal static class TameInteractHelper
{
    private static readonly int InteractMask = LayerMask.GetMask(
        "item", "piece", "piece_nonsolid", "Default", "static_solid", "Default_small",
        "character", "character_net", "terrain", "vehicle");

    public static Character FindAssistTarget(Player player, float maxDistance = 6f)
    {
        return FindEligibleUnderCrosshair(player, maxDistance, assistOnly: true);
    }

    public static Character FindEligibleUnderCrosshair(Player player, float maxDistance, bool assistOnly)
    {
        if (player == null || GameCamera.instance == null)
        {
            return null;
        }

        Vector3 origin = GameCamera.instance.transform.position;
        Vector3 direction = GameCamera.instance.transform.forward;
        Vector3 eye = player.m_eye.position;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 50f, InteractMask);
        Character best = null;
        float bestDistance = maxDistance;

        foreach (RaycastHit hit in hits.OrderBy(h => h.distance))
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.attachedRigidbody != null
                && hit.collider.attachedRigidbody.gameObject == player.gameObject)
            {
                continue;
            }

            Character character = hit.collider.attachedRigidbody != null
                ? hit.collider.attachedRigidbody.GetComponent<Character>()
                : hit.collider.GetComponentInParent<Character>();

            if (character == null || character.IsPlayer() || !TameEligibility.IsEligible(character))
            {
                continue;
            }

            if (assistOnly && !TameEligibility.NeedsCrosshairAssist(character))
            {
                continue;
            }

            float distance = Vector3.Distance(eye, hit.point);
            if (distance <= bestDistance)
            {
                best = character;
                bestDistance = distance;
            }
        }

        return best;
    }

    public static bool TryHandle(Player player, Character character, bool hold, bool alt)
    {
        if (JuvenileSpecies.IsEligibleJuvenile(character))
        {
            return false;
        }

        if (!ModConfig.Enabled.Value || hold || player == null || !TameEligibility.IsEligible(character))
        {
            return false;
        }

        Tameable tameable = character.GetComponent<Tameable>();
        FollowCommands.LastHovered = character;
        AnimalFollowController.EnsureAttached(character);
        JuvenileFollowController.EnsureAttached(character);

        if (alt
            || ZInput.GetButton("AltPlace")
            || Input.GetKey(KeyCode.LeftShift)
            || Input.GetKey(KeyCode.RightShift))
        {
            if (tameable != null)
            {
                tameable.m_commandable = false;
            }

            return false;
        }

        if (!ModConfig.EnableFollow.Value)
        {
            return false;
        }

        if (tameable != null && !CompatPlugins.SkipOurCommandable)
        {
            tameable.m_commandable = true;
        }

        LetsGoRpc.RequestCycleOrder(character, player);
        return true;
    }
}
