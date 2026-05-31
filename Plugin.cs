using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace DarkSpider.MapTracker
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    internal sealed class Plugin : BaseUnityPlugin
    {
        private const string ModGuid = "DarkSpider90.MapTracker";
        private const string ModName = "DarkSpider Map Tracker";
        private const string ModVersion = "0.3.1";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private Harmony _harmony;
        private PluginConfig _config;
        private MapZoomController _zoomController;
        private MapTrackerController _trackerController;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _config = new PluginConfig(Config);
            _zoomController = new MapZoomController(_config);
            _trackerController = new MapTrackerController(_config);

            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"{ModName} {ModVersion} loaded.");
        }

        private void OnDestroy()
        {
            _zoomController?.ResetZoom();
            _trackerController?.Clear();
            _harmony?.UnpatchSelf();
        }

        internal void OnMapAwake(Map map)
        {
            _trackerController?.OnMapAwake(map);
            _zoomController?.CaptureDefaultZoom();
        }

        internal void TickMap(Map map)
        {
            if (map == null || Map.Instance == null)
                return;

            if (!IsLevelReady() || !_config.ModEnabled.Value)
            {
                _trackerController?.SetAllVisible(false);
                _zoomController?.ResetZoom();
                return;
            }

            _zoomController?.Tick();
            _trackerController?.Tick(map);
        }

        internal void RegisterToMap(object source)
        {
            _trackerController?.Register(source);
        }

        internal void ShowToMap(object source)
        {
            _trackerController?.Show(source);
        }

        internal void HideFromMap(object source)
        {
            _trackerController?.Hide(source);
        }

        internal void SwapOnMap(object source, object otherSource)
        {
            _trackerController?.SwapCategory(source, otherSource);
        }

        private static bool IsLevelReady()
        {
            try
            {
                return SemiFunc.RunIsLevel();
            }
            catch
            {
                return false;
            }
        }
    }
}
