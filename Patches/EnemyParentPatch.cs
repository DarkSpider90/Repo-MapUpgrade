using HarmonyLib;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    internal static class EnemyParentPatch
    {
        [HarmonyPatch("Setup")]
        [HarmonyPostfix]
        private static void Setup(EnemyParent __instance)
        {
            Plugin.Instance?.RegisterToMap(__instance);
        }

        [HarmonyPatch("Spawn")]
        [HarmonyPostfix]
        private static void Spawn(EnemyParent __instance)
        {
            Plugin.Instance?.RegisterToMap(__instance);
            Plugin.Instance?.ShowToMap(__instance);
        }

        [HarmonyPatch("SpawnRPC")]
        [HarmonyPostfix]
        private static void SpawnRPC(EnemyParent __instance)
        {
            Plugin.Instance?.RegisterToMap(__instance);
            Plugin.Instance?.ShowToMap(__instance);
        }

        [HarmonyPatch("DespawnRPC")]
        [HarmonyPostfix]
        private static void DespawnRPC(EnemyParent __instance)
        {
            Plugin.Instance?.HideFromMap(__instance);
        }
    }
}
