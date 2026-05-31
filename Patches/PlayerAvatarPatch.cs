using HarmonyLib;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    internal static class PlayerAvatarPatch
    {
        [HarmonyPatch("LateStart")]
        [HarmonyPostfix]
        private static void LateStart(PlayerAvatar __instance)
        {
            if (__instance == null || __instance == PlayerController.instance?.playerAvatarScript)
                return;

            Plugin.Instance?.RegisterToMap(__instance);
            Plugin.Instance?.ShowToMap(__instance);
        }

        [HarmonyPatch("ReviveRPC")]
        [HarmonyPostfix]
        private static void ReviveRPC(PlayerAvatar __instance)
        {
            if (__instance == null || __instance == PlayerController.instance?.playerAvatarScript)
                return;

            Plugin.Instance?.RegisterToMap(__instance);
            Plugin.Instance?.ShowToMap(__instance);
        }

        [HarmonyPatch("PlayerDeathRPC")]
        [HarmonyPostfix]
        private static void PlayerDeathRPC(PlayerAvatar __instance)
        {
            Plugin.Instance?.HideFromMap(__instance);
        }
    }
}
