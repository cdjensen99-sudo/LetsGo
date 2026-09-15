using System;
using BepInEx.Bootstrap;

namespace LetsGo;

internal static class CompatPlugins
{
    internal static bool OffspringPortalLoaded => Chainloader.PluginInfos.ContainsKey(ModConstants.OffspringPortalGuid);

    internal static bool ServersideQoLLoaded =>
        Chainloader.PluginInfos.ContainsKey(ModConstants.ServersideQoLGuid)
        || Chainloader.PluginInfos.ContainsKey(ModConstants.ServersideQoLGuidAlt);

    internal static bool TeleportEverythingLoaded
    {
        get
        {
            if (Chainloader.PluginInfos.ContainsKey(ModConstants.TeleportEverythingGuid))
            {
                return true;
            }

            foreach (var info in Chainloader.PluginInfos.Values)
            {
                if (info.Metadata.GUID.IndexOf("teleporteverything", StringComparison.OrdinalIgnoreCase) >= 0
                    || info.Metadata.Name.IndexOf("TeleportEverything", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static bool SkipOurCommandable =>
        ModConfig.SkipCommandableIfServersideQoL.Value && ServersideQoLLoaded;

    internal static bool SkipOurRecall =>
        ModConfig.SkipRecallIfServersideQoL.Value && ServersideQoLLoaded;

    internal static bool SkipOurPortalFollow =>
        SkipOurRecall
        || (ModConfig.SkipPortalFollowIfTeleportEverything.Value && TeleportEverythingLoaded);
}
