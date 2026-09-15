using UnityEngine;

namespace LetsGo;

internal static class ValheimExtensions
{
    public static ZNetView GetNview(this Character character)
    {
        return character != null ? character.GetComponent<ZNetView>() : null;
    }

    public static ZDO GetZdo(this Character character)
    {
        ZNetView nview = character.GetNview();
        return nview != null && nview.IsValid() ? nview.GetZDO() : null;
    }

    public static string GetPrefabName(this Character character)
    {
        if (character == null)
        {
            return string.Empty;
        }

        return Utils.GetPrefabName(character.gameObject.name);
    }
}
