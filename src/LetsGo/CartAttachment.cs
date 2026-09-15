using System.Collections.Generic;
using UnityEngine;

namespace LetsGo;

internal static class CartAttachment
{
    private const float HitchY = 0.8f;
    private const float DetachSlack = 1.5f;
    private static readonly Dictionary<int, RearCache> RearExtentCache = new Dictionary<int, RearCache>();

    private struct RearCache
    {
        public float Extent;
        public float Time;
    }

    public static bool CanPullCart(Character character)
    {
        if (character == null || character.IsPlayer() || character.IsDead())
        {
            return false;
        }

        if (!character.IsTamed() || !TameEligibility.IsEligible(character))
        {
            return false;
        }

        if (JuvenileSpecies.IsEligibleJuvenile(character) || character.GetComponent<Growup>() != null)
        {
            return false;
        }

        return character.GetRadius() >= ModConfig.CartMinRadius.Value;
    }

    public static Vector3 GetOffset(Character character)
    {
        if (character == null || character.IsPlayer())
        {
            return new Vector3(0f, HitchY, 0f);
        }

        return new Vector3(0f, HitchY, 0f - GetHitchBack(character));
    }

    public static float GetDetachDistance(Character character)
    {
        if (character == null || character.IsPlayer())
        {
            return ModConfig.CartDetachDistancePlayer.Value;
        }

        float fromRadius = character.GetRadius() * ModConfig.CartDetachDistanceFactor.Value;
        return Mathf.Max(fromRadius, GetHitchBack(character) + DetachSlack);
    }

    public static Character FindClosestAttachable(Vagon cart)
    {
        if (cart == null || cart.m_attachPoint == null)
        {
            return null;
        }

        Character closest = null;
        float closestDistance = float.MaxValue;
        foreach (Character character in Character.GetAllCharacters())
        {
            if (!CanPullCart(character))
            {
                continue;
            }

            Vector3 offset = GetOffset(character);
            float distance = Vector3.Distance(character.transform.position + offset, cart.m_attachPoint.position);
            float detach = GetDetachDistance(character);
            if (distance < detach && distance < closestDistance)
            {
                closest = character;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public static Character GetAttached(Vagon cart)
    {
        if (cart == null || !cart.IsAttached())
        {
            return null;
        }

        ConfigurableJoint join = VagonAccess.GetJoin(cart);
        if (join == null || join.connectedBody == null)
        {
            return null;
        }

        return join.connectedBody.GetComponent<Character>();
    }

    public static void Attach(Character target, Vagon cart)
    {
        if (target == null || cart == null)
        {
            return;
        }

        cart.m_attachOffset = GetOffset(target);
        VagonAccess.AttachTo(cart, target.gameObject);
        ConfigurableJoint join = VagonAccess.GetJoin(cart);
        if (join != null && !target.IsPlayer())
        {
            float scale = Mathf.Max(1f, target.GetRadius() * ModConfig.CartPullScalePerRadius.Value);
            join.breakForce = cart.m_breakForce * scale;
        }

        if (!target.IsPlayer())
        {
            SetPullerCollision(cart, target, ignore: true);
        }
    }

    public static void DetachFrom(Character character)
    {
        if (character == null)
        {
            return;
        }

        foreach (Vagon cart in Object.FindObjectsByType<Vagon>(FindObjectsSortMode.None))
        {
            if (GetAttached(cart) == character)
            {
                VagonAccess.Detach(cart);
            }
        }
    }

    public static void SetPullerCollision(Vagon cart, Character character, bool ignore)
    {
        if (ignore && !ModConfig.CartIgnorePullerCollision.Value)
        {
            return;
        }

        if (cart == null || character == null)
        {
            return;
        }

        Collider[] cartColliders = cart.GetComponentsInChildren<Collider>(true);
        Collider[] animalColliders = character.GetComponentsInChildren<Collider>(true);
        foreach (Collider cartCollider in cartColliders)
        {
            if (cartCollider == null || cartCollider.isTrigger)
            {
                continue;
            }

            foreach (Collider animalCollider in animalColliders)
            {
                if (animalCollider == null || animalCollider.isTrigger)
                {
                    continue;
                }

                Physics.IgnoreCollision(cartCollider, animalCollider, ignore);
            }
        }
    }

    private static float GetHitchBack(Character character)
    {
        float radius = character.GetRadius();
        float rear = GetRearExtent(character);
        return Mathf.Max(radius, rear) + ModConfig.CartHitchBackPadding.Value;
    }

    private static float GetRearExtent(Character character)
    {
        int id = character.GetInstanceID();
        if (RearExtentCache.TryGetValue(id, out RearCache cached) && Time.time - cached.Time < 2f)
        {
            return cached.Extent;
        }

        float rear = character.GetRadius();
        Transform root = character.transform;
        Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            foreach (Vector3 corner in corners)
            {
                float behind = -root.InverseTransformPoint(corner).z;
                if (behind > rear)
                {
                    rear = behind;
                }
            }
        }

        RearExtentCache[id] = new RearCache { Extent = rear, Time = Time.time };
        return rear;
    }
}
