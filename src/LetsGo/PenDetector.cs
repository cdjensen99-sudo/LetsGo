using UnityEngine;

namespace LetsGo;

internal static class PenDetector
{
    private static readonly Vector3[] Directions =
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        (Vector3.forward + Vector3.left).normalized,
        (Vector3.forward + Vector3.right).normalized,
        (Vector3.back + Vector3.left).normalized,
        (Vector3.back + Vector3.right).normalized
    };

    private static readonly int StructureMask = LayerMask.GetMask(
        "piece", "piece_nonsolid", "Default", "static_solid", "Default_small");

    public static bool IsPenned(Character character)
    {
        if (character == null)
        {
            return false;
        }

        float distance = Mathf.Max(1f, ModConfig.PackCallPenCheckDistance.Value);
        int needed = Mathf.Clamp(ModConfig.PackCallPenHitMin.Value, 1, 8);
        Vector3 origin = character.transform.position + (Vector3.up * Mathf.Max(0.6f, character.GetRadius()));
        int hits = 0;

        foreach (Vector3 direction in Directions)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, StructureMask)
                && IsPlayerBuiltStructure(hit))
            {
                hits++;
            }
        }

        bool penned = hits >= needed;
        if (penned)
        {
            ModConfig.Log(
                character.GetHoverName() + " penned (" + hits + "/" + Directions.Length + " player-built hits).");
        }

        return penned;
    }

    private static bool IsPlayerBuiltStructure(RaycastHit hit)
    {
        if (hit.collider == null)
        {
            return false;
        }

        if (hit.collider.GetComponentInParent<Character>() != null)
        {
            return false;
        }

        Piece piece = hit.collider.GetComponentInParent<Piece>();
        return piece != null && piece.GetCreator() != 0L;
    }
}
