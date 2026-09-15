using UnityEngine;

namespace LetsGo;

internal static class LocalAuthority
{
    public static bool HasLoaded(Character character)
    {
        ZNetView nview = character?.GetNview();
        return nview != null && nview.IsValid();
    }

    public static bool TryClaim(Character character)
    {
        ZNetView nview = character?.GetNview();
        if (nview == null || !nview.IsValid())
        {
            return false;
        }

        if (!nview.IsOwner())
        {
            nview.ClaimOwnership();
        }

        return nview.IsOwner();
    }

    public static bool IsOwner(Character character)
    {
        ZNetView nview = character?.GetNview();
        return nview != null && nview.IsValid() && nview.IsOwner();
    }
}
