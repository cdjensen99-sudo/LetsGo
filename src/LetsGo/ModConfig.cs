using BepInEx.Configuration;
using UnityEngine;

namespace LetsGo;

internal static class ModConfig
{
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> DebugLogging;
    internal static ConfigEntry<bool> EnableFollow;
    internal static ConfigEntry<bool> EnableCarts;
    internal static ConfigEntry<bool> EnablePackCall;
    internal static ConfigEntry<bool> EnablePortalFollow;
    internal static ConfigEntry<string> ExcludePrefabs;
    internal static ConfigEntry<float> FollowDistanceFactor;
    internal static ConfigEntry<float> MediumFollowDistance;
    internal static ConfigEntry<float> CartMinRadius;
    internal static ConfigEntry<float> CartHitchBackPadding;
    internal static ConfigEntry<bool> CartIgnorePullerCollision;
    internal static ConfigEntry<float> CartDetachDistanceFactor;
    internal static ConfigEntry<float> CartDetachDistancePlayer;
    internal static ConfigEntry<float> CartPullScalePerRadius;
    internal static ConfigEntry<float> PackCallRadius;
    internal static ConfigEntry<int> PackCallMax;
    internal static ConfigEntry<bool> PackCallIgnorePenned;
    internal static ConfigEntry<float> PackCallPenCheckDistance;
    internal static ConfigEntry<int> PackCallPenHitMin;
    internal static ConfigEntry<float> PortalFollowRadius;
    internal static ConfigEntry<bool> SkipCommandableIfServersideQoL;
    internal static ConfigEntry<bool> SkipRecallIfServersideQoL;
    internal static ConfigEntry<bool> SkipPortalFollowIfTeleportEverything;
    internal static ConfigEntry<KeyboardShortcut> PackCallKey;

    internal static void Bind(ConfigFile config)
    {
        Enabled = config.Bind("General", "Enabled", true, "Master switch. When false, no patches run.");
        DebugLogging = config.Bind("General", "DebugLogging", false, "Extra BepInEx logs.");
        EnableFollow = config.Bind("General", "EnableFollow", true, "Follow / Stay on tamed creatures, including young.");
        EnableCarts = config.Bind("General", "EnableCarts", true, "Hitch vanilla carts to adult boar-sized or larger tames.");
        EnablePackCall = config.Bind("General", "EnablePackCall", true, "Hotkey to call nearby tames that are not inside a pen.");
        EnablePortalFollow = config.Bind("General", "EnablePortalFollow", true, "Following tames travel vanilla portals with you.");
        ExcludePrefabs = config.Bind(
            "General",
            "ExcludePrefabs",
            "",
            "Comma-separated prefab names that never receive orders (for problem AllTameable bosses).");

        FollowDistanceFactor = config.Bind(
            "Follow",
            "FollowDistanceFactor",
            3f,
            "Follow stop distance = max(MediumFollowDistance, radius * this). Larger beasts stay farther back.");
        MediumFollowDistance = config.Bind(
            "Follow",
            "MediumFollowDistance",
            3f,
            "Minimum follow stop distance for small/medium tames.");

        CartMinRadius = config.Bind(
            "Carts",
            "MinRadius",
            0.45f,
            "Smallest adult tame radius that can hitch a cart. Adult boar is the floor; hens and young never pull.");
        CartHitchBackPadding = config.Bind(
            "Carts",
            "HitchBackPadding",
            1.25f,
            "Extra meters behind the animal's rear so the vanilla cart is not under a lox, moose, or Asksvin.");
        CartIgnorePullerCollision = config.Bind(
            "Carts",
            "IgnorePullerCollision",
            true,
            "Ignore physics between the cart and the animal pulling it so turns and body clips do not damage the cart.");
        CartDetachDistanceFactor = config.Bind(
            "Carts",
            "DetachDistanceFactor",
            3.5f,
            "Detach distance is animal radius times this factor.");
        CartDetachDistancePlayer = config.Bind(
            "Carts",
            "DetachDistancePlayer",
            2f,
            "How far the player can be from the cart while pulling.");
        CartPullScalePerRadius = config.Bind(
            "Carts",
            "PullScalePerRadius",
            2f,
            "Joint break force multiplier is max(1, radius * this). Larger adults haul more weight.");

        PackCallRadius = config.Bind("PackCall", "Radius", 50f, "How far the pack call reaches.");
        PackCallMax = config.Bind("PackCall", "MaxTames", 10, "Maximum tames one call can take (1-50).");
        PackCallIgnorePenned = config.Bind(
            "PackCall",
            "IgnorePenned",
            true,
            "If true, skip tames enclosed by player-built structures (fences, walls, barns). Stay in the open still answers.");
        PackCallPenCheckDistance = config.Bind(
            "PackCall",
            "PenCheckDistance",
            6f,
            "How far to look for player-built structures when deciding if a tame is penned.");
        PackCallPenHitMin = config.Bind(
            "PackCall",
            "PenHitMin",
            5,
            "A tame is penned when this many of 8 directions hit a player-built structure (1-8).");
        PackCallKey = config.Bind(
            "Controls",
            "PackCallKey",
            new KeyboardShortcut(KeyCode.T),
            "Call nearby eligible tames to Follow.");

        PortalFollowRadius = config.Bind(
            "Portal",
            "FollowRadius",
            12f,
            "Following tames within this radius of the source portal come with you.");

        SkipCommandableIfServersideQoL = config.Bind(
            "Compatibility",
            "SkipCommandableIfServersideQoL",
            true,
            "If ServersideQoL is loaded, do not also force m_commandable.");
        SkipRecallIfServersideQoL = config.Bind(
            "Compatibility",
            "SkipRecallIfServersideQoL",
            true,
            "If ServersideQoL is loaded, skip portal-follow (its TeleportFollow already warps distant followers).");
        SkipPortalFollowIfTeleportEverything = config.Bind(
            "Compatibility",
            "SkipPortalFollowIfTeleportEverything",
            true,
            "If TeleportEverything is loaded, skip our portal-follow to avoid double-teleport.");
    }

    internal static void Log(string message)
    {
        if (!DebugLogging.Value)
        {
            return;
        }

        LetsGoPlugin.Log.LogInfo(message);
    }
}
