using HarmonyLib;
using UnityEngine;

namespace LetsGo.Patches;

[HarmonyPatch(typeof(TextInput), nameof(TextInput.RequestText))]
internal static class TameRenameRequestTextPatch
{
    private static void Prefix(TextReceiver sign, string topic, ref int charLimit)
    {
        if (!TameRename.ShouldRaiseLimit(sign, topic))
        {
            return;
        }

        charLimit = TameRename.ClampLimit();
    }
}

[HarmonyPatch(typeof(TextInput), "Show")]
internal static class TameRenameShowPatch
{
    private static void Prefix(string topic, ref int charLimit)
    {
        if (!TameRename.ShouldRaiseLimit(null, topic))
        {
            return;
        }

        charLimit = TameRename.ClampLimit();
    }
}

[HarmonyPatch(typeof(Tameable), nameof(Tameable.SetText))]
internal static class TameRenameSetTextPatch
{
    private static void Prefix(ref string text)
    {
        if (!ModConfig.Enabled.Value || string.IsNullOrEmpty(text))
        {
            return;
        }

        int max = TameRename.ClampLimit();
        if (text.Length > max)
        {
            text = text.Substring(0, max);
        }
    }
}

internal static class TameRename
{
    internal static int ClampLimit()
    {
        int configured = ModConfig.TameNameMaxLength != null ? ModConfig.TameNameMaxLength.Value : 100;
        return Mathf.Clamp(configured, 10, 100);
    }

    internal static bool ShouldRaiseLimit(TextReceiver sign, string topic)
    {
        if (!ModConfig.Enabled.Value)
        {
            return false;
        }

        if (sign is Tameable)
        {
            return true;
        }

        return !string.IsNullOrEmpty(topic) && topic.IndexOf("rename", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
