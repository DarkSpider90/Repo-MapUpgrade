using System;
using UnityEngine;

namespace DarkSpider.MapTracker
{
    internal sealed class MapZoomController
    {
        private readonly PluginConfig _config;

        internal MapZoomController(PluginConfig config)
        {
            _config = config;
        }

        internal void CaptureDefaultZoom()
        {
            if (Patches.MapPatch.MapCamera != null && Patches.MapPatch.DefaultMapZoom <= 0f)
                Patches.MapPatch.DefaultMapZoom = Patches.MapPatch.MapCamera.orthographicSize;
        }

        internal void Tick()
        {
            HandleZoomInput();
            ApplyZoom();
        }

        internal void ResetZoom()
        {
            if (Patches.MapPatch.MapCamera == null || Patches.MapPatch.DefaultMapZoom <= 0f)
                return;

            Patches.MapPatch.MapCamera.orthographicSize = Patches.MapPatch.DefaultMapZoom;
        }

        private void ApplyZoom()
        {
            if (Patches.MapPatch.MapCamera == null || Patches.MapPatch.DefaultMapZoom <= 0f)
                return;

            Patches.MapPatch.MapCamera.orthographicSize =
                Patches.MapPatch.DefaultMapZoom * Mathf.Clamp(_config.MapZoomMultiplier.Value, 0.25f, 5f);
        }

        private void HandleZoomInput()
        {
            if (!IsMapOpen())
                return;

            MapZoomInputMode inputMode = _config.ZoomInputMode.Value;
            if (inputMode == MapZoomInputMode.Disabled)
                return;

            float value = _config.MapZoomMultiplier.Value;
            float step = Mathf.Clamp(_config.ZoomStep.Value, 0.05f, 1f);
            bool changed = false;

            if (inputMode == MapZoomInputMode.MouseWheel || inputMode == MapZoomInputMode.MouseWheelAndKeyboard)
            {
                float wheel = GetMouseWheelDelta();
                if (Mathf.Abs(wheel) > 0.01f)
                {
                    value -= wheel * step;
                    changed = true;
                }
            }

            if (inputMode == MapZoomInputMode.Keyboard || inputMode == MapZoomInputMode.MouseWheelAndKeyboard)
            {
                if (IsConfiguredKeyDown(_config.ZoomInKey.Value))
                {
                    value -= step;
                    changed = true;
                }

                if (IsConfiguredKeyDown(_config.ZoomOutKey.Value))
                {
                    value += step;
                    changed = true;
                }
            }

            if (changed)
                _config.MapZoomMultiplier.Value = Mathf.Clamp(value, 0.25f, 5f);
        }

        private static bool IsMapOpen()
        {
            if (Map.Instance != null && Map.Instance.Active)
                return true;

            Camera mapCamera = Patches.MapPatch.MapCamera;
            return mapCamera != null && mapCamera.gameObject.activeInHierarchy && mapCamera.enabled;
        }

        private static float GetMouseWheelDelta()
        {
            return Input.mouseScrollDelta.y;
        }

        private static bool IsConfiguredKeyDown(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return false;

            string normalized = NormalizeKeyName(keyName);
            if (!Enum.TryParse(normalized, true, out KeyCode keyCode))
                return false;

            return Input.GetKeyDown(keyCode);
        }

        private static string NormalizeKeyName(string keyName)
        {
            string normalized = keyName.Trim();

            if (normalized == "+" || normalized == "=")
                return "Equals";

            if (normalized == "-" || normalized == "_")
                return "Minus";

            if (normalized.Equals("Plus", StringComparison.OrdinalIgnoreCase))
                return "Equals";

            return normalized;
        }
    }
}
