using UnityEngine;

namespace LetsGo;

internal sealed class AnimalFollowController : MonoBehaviour
{
    public static void EnsureAttached(Character character)
    {
        if (character == null || character.GetComponent<AnimalAI>() == null)
        {
            return;
        }

        if (character.GetComponent<AnimalFollowController>() == null)
        {
            character.gameObject.AddComponent<AnimalFollowController>();
        }
    }

    public static bool TryApply(AnimalAI animalAi, float dt)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableFollow.Value || animalAi == null)
        {
            return false;
        }

        Character character = animalAi.GetComponent<Character>();
        ZNetView nview = animalAi.GetComponent<ZNetView>();
        if (character == null || nview == null || !nview.IsValid() || !nview.IsOwner())
        {
            return false;
        }

        if (!TameEligibility.IsEligible(character) || FollowCommands.GetOrder(character) != TameOrder.Follow)
        {
            return false;
        }

        ZDO zdo = character.GetZdo();
        if (zdo == null)
        {
            return false;
        }

        Player target = TameEligibility.ResolvePlayer(zdo.GetZDOID(ZdoFields.FollowTarget));
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
}
