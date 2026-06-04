using System;
using BepInEx.Configuration;
using UnityEngine;

namespace DarkSpider.MapTracker
{
    internal enum ArrowColorPreset
    {
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Blue,
        Purple,
        Pink,
        White,
        Black,
        Gray,
        CustomHex
    }

    internal enum PlayerArrowColorMode
    {
        PlayerHeadColor,
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Blue,
        Purple,
        Pink,
        White,
        Black,
        Gray,
        CustomHex
    }

    internal enum MapZoomInputMode
    {
        MouseWheel,
        Keyboard,
        MouseWheelAndKeyboard,
        Disabled
    }

    internal sealed class PluginConfig
    {
        internal ConfigEntry<bool> ModEnabled { get; }
        internal ConfigEntry<bool> ShowEnemies { get; }
        internal ConfigEntry<ArrowColorPreset> EnemyColor { get; }
        internal ConfigEntry<string> EnemyCustomHex { get; }
        internal ConfigEntry<float> EnemyArrowScale { get; }

        internal ConfigEntry<bool> ShowPlayers { get; }
        internal ConfigEntry<PlayerArrowColorMode> PlayerColor { get; }
        internal ConfigEntry<string> PlayerCustomHex { get; }
        internal ConfigEntry<float> PlayerArrowScale { get; }

        internal ConfigEntry<bool> OutlineEnabled { get; }
        internal ConfigEntry<ArrowColorPreset> OutlineColor { get; }
        internal ConfigEntry<string> OutlineCustomHex { get; }
        internal ConfigEntry<float> OutlineScale { get; }

        internal ConfigEntry<float> MapZoomMultiplier { get; }
        internal ConfigEntry<MapZoomInputMode> ZoomInputMode { get; }
        internal ConfigEntry<string> ZoomInKey { get; }
        internal ConfigEntry<string> ZoomOutKey { get; }
        internal ConfigEntry<float> ZoomStep { get; }

        internal PluginConfig(ConfigFile config)
        {
            ModEnabled = config.Bind("General", "ModEnabled", true, "Enable or disable the whole map tracker mod.");

            ShowEnemies = config.Bind("Enemies", "ShowEnemyArrows", true, "Show enemy arrows on the map.");
            EnemyColor = config.Bind("Enemies", "EnemyArrowColor", ArrowColorPreset.Red, "Enemy arrow color. Select CustomHex to use EnemyCustomHex.");
            EnemyCustomHex = config.Bind("Enemies", "EnemyCustomHex", "#FF0000", "Custom enemy arrow color used when EnemyArrowColor is CustomHex.");
            EnemyArrowScale = config.Bind("Enemies", "EnemyArrowScale", 0.25f, new ConfigDescription("Enemy arrow size. 0.25 is the original small triangle size.", new AcceptableValueRange<float>(0.25f, 3f)));

            ShowPlayers = config.Bind("Players", "ShowPlayerArrows", true, "Show player arrows on the map.");
            PlayerColor = config.Bind("Players", "PlayerArrowColor", PlayerArrowColorMode.PlayerHeadColor, "Player arrow color. PlayerHeadColor uses HeadTopMesh color. Select CustomHex to use PlayerCustomHex.");
            PlayerCustomHex = config.Bind("Players", "PlayerCustomHex", "#00FFFF", "Custom player arrow color used when PlayerArrowColor is CustomHex.");
            PlayerArrowScale = config.Bind("Players", "PlayerArrowScale", 0.25f, new ConfigDescription("Player arrow size. 0.25 is the original small triangle size.", new AcceptableValueRange<float>(0.25f, 3f)));

            OutlineEnabled = config.Bind("Outline", "OutlineEnabled", true, "Draw an outline behind map arrows. Useful for white/gray player colors.");
            OutlineColor = config.Bind("Outline", "OutlineColor", ArrowColorPreset.Black, "Outline color. Select CustomHex to use OutlineCustomHex.");
            OutlineCustomHex = config.Bind("Outline", "OutlineCustomHex", "#000000", "Custom outline color used when OutlineColor is CustomHex.");
            OutlineScale = config.Bind("Outline", "OutlineScale", 1.35f, new ConfigDescription("Outline size relative to the main arrow.", new AcceptableValueRange<float>(1f, 2f)));

            MapZoomMultiplier = config.Bind("Map Zoom", "MapZoomMultiplier", 1f, new ConfigDescription("1.0 = vanilla. Bigger values zoom out, smaller values zoom in.", new AcceptableValueRange<float>(0.25f, 5f)));
            ZoomInputMode = config.Bind("Map Zoom", "ZoomInputMode", MapZoomInputMode.MouseWheel, "How map zoom is controlled while the map is open.");
            ZoomInKey = config.Bind("Map Zoom", "ZoomInKey", "Equals", "Keyboard zoom-in key while the map is open. Examples: Equals, KeypadPlus, PageUp.");
            ZoomOutKey = config.Bind("Map Zoom", "ZoomOutKey", "Minus", "Keyboard zoom-out key while the map is open. Examples: Minus, KeypadMinus, PageDown.");
            ZoomStep = config.Bind("Map Zoom", "ZoomStep", 0.1f, new ConfigDescription("How much each mouse-wheel notch or hotkey press changes MapZoomMultiplier.", new AcceptableValueRange<float>(0.05f, 1f)));
        }

        internal Color GetEnemyColor()
        {
            return ColorTools.ResolvePreset(EnemyColor.Value, EnemyCustomHex.Value, Color.red);
        }

        internal Color GetOutlineColor()
        {
            return ColorTools.ResolvePreset(OutlineColor.Value, OutlineCustomHex.Value, Color.black);
        }
    }

    internal static class ColorTools
    {
        internal static Color ResolvePreset(ArrowColorPreset preset, string customHex, Color fallback)
        {
            switch (preset)
            {
                case ArrowColorPreset.Red: return Color.red;
                case ArrowColorPreset.Orange: return new Color(1f, 0.5f, 0f, 1f);
                case ArrowColorPreset.Yellow: return Color.yellow;
                case ArrowColorPreset.Green: return Color.green;
                case ArrowColorPreset.Cyan: return new Color(0f, 0.85f, 1f, 1f);
                case ArrowColorPreset.Blue: return new Color(0.05f, 0.2f, 1f, 1f);
                case ArrowColorPreset.Purple: return new Color(0.55f, 0f, 1f, 1f);
                case ArrowColorPreset.Pink: return new Color(1f, 0.1f, 0.65f, 1f);
                case ArrowColorPreset.White: return Color.white;
                case ArrowColorPreset.Black: return Color.black;
                case ArrowColorPreset.Gray: return new Color(0.28f, 0.28f, 0.28f, 1f);
                case ArrowColorPreset.CustomHex: return ParseHex(customHex, fallback);
                default: return fallback;
            }
        }

        internal static Color ResolvePlayerColor(PlayerArrowColorMode mode, string customHex, PlayerAvatar playerAvatar)
        {
            if (mode == PlayerArrowColorMode.PlayerHeadColor)
            {
                Color? headColor = ReflectionCache.TryReadPlayerHeadTopColor(playerAvatar);
                return headColor ?? Color.cyan;
            }

            if (mode == PlayerArrowColorMode.CustomHex)
                return ParseHex(customHex, Color.cyan);

            if (Enum.TryParse(mode.ToString(), out ArrowColorPreset preset))
                return ResolvePreset(preset, customHex, Color.cyan);

            return Color.cyan;
        }

        private static Color ParseHex(string htmlColor, Color fallback)
        {
            if (!string.IsNullOrWhiteSpace(htmlColor) && ColorUtility.TryParseHtmlString(htmlColor.Trim(), out Color color))
                return color;

            return fallback;
        }
    }
}
