using HarmonyLib;
using UnityEngine;

namespace DarkSpider.MapTracker.Patches
{
    [HarmonyPatch(typeof(Map))]
    internal static class MapPatch
    {
        internal static Camera MapCamera;
        internal static float DefaultMapZoom;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void Awake(Map __instance, ref Transform ___playerTransformTarget)
        {
            if (___playerTransformTarget != null)
            {
                MapCamera = ___playerTransformTarget.GetComponentInChildren<Camera>();
                if (MapCamera != null)
                    DefaultMapZoom = MapCamera.orthographicSize;
            }

            Plugin.Instance?.OnMapAwake(__instance);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(Map __instance)
        {
            Plugin.Instance?.TickMap(__instance);
        }
    }
}
