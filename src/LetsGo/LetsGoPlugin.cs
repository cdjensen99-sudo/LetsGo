using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LetsGo;

[BepInPlugin(ModConstants.ModGuid, ModConstants.ModName, ModConstants.ModVersion)]
[BepInDependency(ModConstants.OffspringPortalGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInIncompatibility(ModConstants.BeastsOfBurdenGuid)]
public sealed class LetsGoPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;
        ModConfig.Bind(Config);

        if (!ModConfig.Enabled.Value)
        {
            Log.LogInfo($"{ModConstants.ModName} {ModConstants.ModVersion} is disabled in config.");
            return;
        }

        _harmony = new Harmony(ModConstants.ModGuid);
        PatchAllSafe();
        Log.LogInfo($"{ModConstants.ModName} {ModConstants.ModVersion} loaded ({ModConstants.BuildLabel}, dll {GetDllTimestamp()}).");
    }

    private void Update()
    {
        if (!ModConfig.Enabled.Value || Player.m_localPlayer == null)
        {
            return;
        }

        if (!CanTakeHotkeys())
        {
            return;
        }

        if (ModConfig.EnablePackCall.Value && ModConfig.PackCallKey.Value.IsDown())
        {
            PackCall.TryCallFromLocal();
        }

        if (ModConfig.EnablePackStay.Value && ModConfig.PackStayKey.Value.IsDown())
        {
            PackStay.TryStayFromLocal();
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    private void PatchAllSafe()
    {
        foreach (Type type in typeof(LetsGoPlugin).Assembly.GetTypes())
        {
            try
            {
                _harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to patch " + type.FullName + ": " + ex);
            }
        }
    }

    private static string GetDllTimestamp()
    {
        string path = Assembly.GetExecutingAssembly().Location;
        return File.Exists(path)
            ? File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm:ss")
            : "unknown";
    }

    private static bool CanTakeHotkeys()
    {
        Player player = Player.m_localPlayer;
        if (player == null || player.IsDead() || player.InCutscene() || player.IsTeleporting())
        {
            return false;
        }

        if (Console.IsVisible()
            || TextInput.IsVisible()
            || InventoryGui.IsVisible()
            || Minimap.IsOpen()
            || StoreGui.IsVisible()
            || Menu.IsVisible()
            || GameCamera.InFreeFly()
            || PlayerCustomizaton.IsBarberGuiVisible())
        {
            return false;
        }

        if (TextViewer.instance != null && TextViewer.instance.IsVisible())
        {
            return false;
        }

        if (Chat.instance != null && Chat.instance.HasFocus())
        {
            return false;
        }

        return !IsTextFieldFocused();
    }

    private static bool IsTextFieldFocused()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null)
        {
            return false;
        }

        Transform current = selected.transform;
        while (current != null)
        {
            foreach (Component component in current.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().Name;
                if (typeName == "InputField" || typeName == "TMP_InputField")
                {
                    return true;
                }
            }

            current = current.parent;
        }

        return false;
    }
}
