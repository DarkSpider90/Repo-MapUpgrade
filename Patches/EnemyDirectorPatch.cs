using HarmonyLib;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(EnemyDirector))]
    internal static class EnemyDirectorPatch
    {
        [HarmonyPatch("FirstSpawnPointAdd")]
        [HarmonyPostfix]
        private static void FirstSpawnPointAdd(EnemyParent _enemyParent)
        {
            if (_enemyParent == null)
                return;

            Plugin.Instance?.RegisterToMap(_enemyParent);
            Plugin.Instance?.ShowToMap(_enemyParent);
        }
    }
}
