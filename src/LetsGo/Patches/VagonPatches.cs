using HarmonyLib;
using UnityEngine;

namespace LetsGo.Patches;

[HarmonyPatch(typeof(Vagon), "Detach")]
internal static class VagonDetachPatch
{
    private static void Prefix(Vagon __instance, out Character __state)
    {
        __state = CartAttachment.GetAttached(__instance);
    }

    private static void Postfix(Vagon __instance, Character __state)
    {
        CartAttachment.SetPullerCollision(__instance, __state, ignore: false);
    }
}

[HarmonyPatch(typeof(Vagon), "LateUpdate")]
internal static class VagonLateUpdatePatch
{
    private static void Prefix(Vagon __instance)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableCarts.Value)
        {
            return;
        }

        ConfigurableJoint join = VagonAccess.GetJoin(__instance);
        if (join != null && join.connectedBody == null)
        {
            VagonAccess.Detach(__instance);
        }
    }
}

[HarmonyPatch(typeof(Vagon), nameof(Vagon.InUse))]
internal static class VagonInUsePatch
{
    private static bool Prefix(Vagon __instance, ref bool __result)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableCarts.Value)
        {
            return true;
        }

        if (__instance.m_container != null && __instance.m_container.IsInUse())
        {
            __result = true;
            return false;
        }

        if (__instance.IsAttached())
        {
            Character attached = CartAttachment.GetAttached(__instance);
            __result = attached != null && attached.IsPlayer();
            return false;
        }

        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Vagon), "Update")]
internal static class VagonUpdatePatch
{
    private static bool Prefix(Vagon __instance)
    {
        if (!ModConfig.Enabled.Value || !ModConfig.EnableCarts.Value)
        {
            return true;
        }

        ZNetView nview = VagonAccess.GetNview(__instance);
        if (nview == null || !nview.IsValid())
        {
            return false;
        }

        VagonAccess.UpdateAudio(__instance, Time.deltaTime);
        if (!nview.IsOwner())
        {
            if (__instance.IsAttached())
            {
                VagonAccess.Detach(__instance);
            }

            VagonAccess.UpdateSnow(__instance);
            return false;
        }

        Player requester = VagonAccess.GetUseRequester(__instance);
        if (requester != null)
        {
            if (__instance.IsAttached())
            {
                VagonAccess.Detach(__instance);
            }
            else
            {
                Character closest = CartAttachment.FindClosestAttachable(__instance);
                if (closest != null)
                {
                    CartAttachment.Attach(closest, __instance);
                }
                else if (VagonAccess.CanAttach(__instance, requester.gameObject))
                {
                    CartAttachment.Attach(requester, __instance);
                }
                else
                {
                    requester.Message(MessageHud.MessageType.Center, "$msg_cart_incorrectposition");
                }
            }

            VagonAccess.SetUseRequester(__instance, null);
        }

        if (__instance.IsAttached())
        {
            Character attached = CartAttachment.GetAttached(__instance);
            __instance.m_detachDistance = CartAttachment.GetDetachDistance(attached);
            ConfigurableJoint join = VagonAccess.GetJoin(__instance);
            GameObject connected = join != null && join.connectedBody != null
                ? join.connectedBody.gameObject
                : null;
            if (connected == null || !VagonAccess.CanAttach(__instance, connected))
            {
                VagonAccess.Detach(__instance);
            }
        }

        VagonAccess.UpdateSnow(__instance);
        return false;
    }
}
