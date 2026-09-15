using UnityEngine;

namespace LetsGo;

internal sealed class JuvenileFollowController : MonoBehaviour
{
    public static bool TryApplyAnimalFollow(AnimalAI animalAi, float dt)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableFollow.Value || animalAi == null)
        {
            return false;
        }

        Character character = animalAi.GetComponent<Character>();
        ZNetView nview = animalAi.GetComponent<ZNetView>();
        if (character == null || nview == null || !nview.IsValid())
        {
            return false;
        }

        if (!nview.IsOwner())
        {
            return false;
        }

        if (!JuvenileSpecies.IsEligibleJuvenile(character) || !character.IsTamed())
        {
            return false;
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

        Player target = TameEligibility.ResolvePlayer(followId);
        if (target == null)
        {
            return false;
        }

        Vector3 offset = target.transform.position - character.transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;
        float stop = FollowCommands.GetFollowStopDistance(character);
        if (distance < stop)
        {
            animalAi.StopMoving();
            return true;
        }

        animalAi.MoveTowards(offset / distance, distance > stop * 3f);
        return true;
    }

    public static void EnsureAttached(Character character)
    {
        if (character == null || character.GetComponent<AnimalAI>() == null)
        {
            return;
        }

        if (character.GetComponent<JuvenileFollowController>() == null)
        {
            character.gameObject.AddComponent<JuvenileFollowController>();
        }
    }
}
