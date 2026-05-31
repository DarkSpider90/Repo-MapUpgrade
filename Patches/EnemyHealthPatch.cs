using HarmonyLib;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(EnemyHealth))]
    internal static class EnemyHealthPatch
    {
        [HarmonyPatch("DeathRPC")]
        [HarmonyPostfix]
        private static void DeathRPC(Enemy ___enemy)
        {
            EnemyParent enemyParent = ReflectionCache.GetEnemyParent(___enemy);
            if (enemyParent != null)
                Plugin.Instance?.HideFromMap(enemyParent);
        }
    }
}
