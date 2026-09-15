using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace LetsGo;

internal static class VagonAccess
{
    private static readonly AccessTools.FieldRef<Vagon, ConfigurableJoint> AttachJoin =
        AccessTools.FieldRefAccess<Vagon, ConfigurableJoint>("m_attachJoin");

    private static readonly AccessTools.FieldRef<Vagon, Humanoid> UseRequester =
        AccessTools.FieldRefAccess<Vagon, Humanoid>("m_useRequester");

    private static readonly AccessTools.FieldRef<Vagon, ZNetView> Nview =
        AccessTools.FieldRefAccess<Vagon, ZNetView>("m_nview");

    private static readonly MethodInfo AttachToMethod = AccessTools.Method(typeof(Vagon), "AttachTo", new[] { typeof(GameObject) });
    private static readonly MethodInfo DetachMethod = AccessTools.Method(typeof(Vagon), "Detach");
    private static readonly MethodInfo CanAttachMethod = AccessTools.Method(typeof(Vagon), "CanAttach", new[] { typeof(GameObject) });
    private static readonly MethodInfo UpdateAudioMethod = AccessTools.Method(typeof(Vagon), "UpdateAudio", new[] { typeof(float) });
    private static readonly MethodInfo UpdateSnowMethod = AccessTools.Method(typeof(Vagon), "UpdateSnow");

    internal static ConfigurableJoint GetJoin(Vagon cart) => AttachJoin(cart);

    internal static Player GetUseRequester(Vagon cart) => UseRequester(cart) as Player;

    internal static void SetUseRequester(Vagon cart, Player player) => UseRequester(cart) = player;

    internal static ZNetView GetNview(Vagon cart) => Nview(cart);

    internal static void AttachTo(Vagon cart, GameObject target)
    {
        AttachToMethod?.Invoke(cart, new object[] { target });
    }

    internal static void Detach(Vagon cart)
    {
        DetachMethod?.Invoke(cart, null);
    }

    internal static bool CanAttach(Vagon cart, GameObject target)
    {
        object result = CanAttachMethod?.Invoke(cart, new object[] { target });
        return result is bool ok && ok;
    }

    internal static void UpdateAudio(Vagon cart, float dt)
    {
        UpdateAudioMethod?.Invoke(cart, new object[] { dt });
    }

    internal static void UpdateSnow(Vagon cart)
    {
        UpdateSnowMethod?.Invoke(cart, null);
    }
}

internal static class BaseAIAccess
{
    private static readonly AccessTools.FieldRef<BaseAI, Character> CharacterField =
        AccessTools.FieldRefAccess<BaseAI, Character>("m_character");

    private static readonly MethodInfo MoveToMethod = AccessTools.Method(
        typeof(BaseAI),
        "MoveTo",
        new[] { typeof(float), typeof(Vector3), typeof(float), typeof(bool) });

    internal static Character GetCharacter(BaseAI ai) => ai != null ? CharacterField(ai) : null;

    internal static bool MoveTo(BaseAI ai, float dt, Vector3 point, float distance, bool run)
    {
        object result = MoveToMethod?.Invoke(ai, new object[] { dt, point, distance, run });
        return result is bool ok && ok;
    }
}
