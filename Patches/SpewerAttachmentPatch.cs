using HarmonyLib;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(EnemySlowMouth))]
    internal static class SpewerAttachmentPatch
    {
        [HarmonyPatch("UpdateStateRPC")]
        [HarmonyPostfix]
        private static void UpdateStateRPC(EnemySlowMouth __instance, Enemy ___enemy)
        {
            if (__instance == null || ___enemy == null)
                return;

            PlayerAvatar playerTarget = ReflectionCache.GetSlowMouthTarget(__instance);
            EnemyParent enemyParent = ReflectionCache.GetEnemyParent(___enemy);

            if (playerTarget == null || enemyParent == null)
                return;

            if (__instance.currentState == EnemySlowMouth.State.Attached)
            {
                Plugin.Instance?.HideFromMap(enemyParent);
                Plugin.Instance?.SwapOnMap(enemyParent, playerTarget);
            }
            else if (__instance.currentState == EnemySlowMouth.State.Detach)
            {
                Plugin.Instance?.SwapOnMap(playerTarget, enemyParent);
                Plugin.Instance?.ShowToMap(enemyParent);
            }
        }
    }
}
