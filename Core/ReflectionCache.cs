using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DarkSpider.MapTracker
{
    internal static class ReflectionCache
    {
        private static readonly FieldInfo EnemyParentEnemyField = AccessTools.Field(typeof(EnemyParent), "Enemy");
        private static readonly FieldInfo EnemyEnemyParentField = AccessTools.Field(typeof(Enemy), "EnemyParent");
        private static readonly FieldInfo SlowMouthTargetField = AccessTools.Field(typeof(EnemySlowMouth), "playerTarget");
        private static readonly FieldInfo PlayerDisabledField = AccessTools.Field(typeof(PlayerAvatar), "isDisabled");

        private static readonly FieldInfo PlayerCosmeticsField = AccessTools.Field(typeof(PlayerAvatar), "playerCosmetics");
        private static readonly FieldInfo PlayerCosmeticsColorsField = AccessTools.Field(typeof(PlayerCosmetics), "colorsEquipped");
        private static readonly FieldInfo MetaManagerInstanceField = AccessTools.Field(typeof(MetaManager), "instance");
        private static readonly FieldInfo MetaManagerColorsField = AccessTools.Field(typeof(MetaManager), "colors");
        private static readonly FieldInfo SemiColorColorField = AccessTools.Field(typeof(SemiColor), "color");

        internal static Enemy GetEnemy(EnemyParent enemyParent)
        {
            if (enemyParent == null)
                return null;

            try
            {
                return EnemyParentEnemyField?.GetValue(enemyParent) as Enemy;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read EnemyParent.Enemy: {exception.Message}");
                return null;
            }
        }

        internal static EnemyParent GetEnemyParent(Enemy enemy)
        {
            if (enemy == null)
                return null;

            try
            {
                return EnemyEnemyParentField?.GetValue(enemy) as EnemyParent;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read Enemy.EnemyParent: {exception.Message}");
                return null;
            }
        }

        internal static PlayerAvatar GetSlowMouthTarget(EnemySlowMouth slowMouth)
        {
            if (slowMouth == null)
                return null;

            try
            {
                return SlowMouthTargetField?.GetValue(slowMouth) as PlayerAvatar;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read EnemySlowMouth.playerTarget: {exception.Message}");
                return null;
            }
        }

        internal static bool IsPlayerDisabled(PlayerAvatar playerAvatar)
        {
            if (playerAvatar == null)
                return true;

            try
            {
                object value = PlayerDisabledField?.GetValue(playerAvatar);
                return value is bool disabled && disabled;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read PlayerAvatar.isDisabled: {exception.Message}");
                return false;
            }
        }

        internal static Color? TryReadPlayerHeadTopColor(PlayerAvatar playerAvatar)
        {
            try
            {
                PlayerCosmetics cosmetics = PlayerCosmeticsField?.GetValue(playerAvatar) as PlayerCosmetics;
                int[] equippedColors = PlayerCosmeticsColorsField?.GetValue(cosmetics) as int[];
                if (equippedColors == null || equippedColors.Length <= 5)
                    return null;

                int colorIndex = equippedColors[5];

                MetaManager metaManager = MetaManagerInstanceField?.GetValue(null) as MetaManager;
                List<SemiColor> metaColors = MetaManagerColorsField?.GetValue(metaManager) as List<SemiColor>;
                if (metaColors == null || metaColors.Count == 0)
                    return null;

                if (colorIndex < 0 || colorIndex >= metaColors.Count)
                    colorIndex = 0;

                object value = SemiColorColorField?.GetValue(metaColors[colorIndex]);
                if (value is Color color)
                    return color;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read player HeadTopMesh color: {exception.Message}");
            }

            return null;
        }
    }

}
