using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DarkSpider.MapTracker
{
    internal sealed class MapTrackerController
    {
        private const float PlayerSafetyScanInterval = 1f;

        private readonly PluginConfig _config;
        private readonly List<TrackedMapEntity> _enemyTrackers = new List<TrackedMapEntity>();
        private readonly List<TrackedMapEntity> _playerTrackers = new List<TrackedMapEntity>();
        private readonly Sprite _arrowSprite;

        private float _nextPlayerSafetyScan;
        private Map _lastMap;

        internal MapTrackerController(PluginConfig config)
        {
            _config = config;
            _arrowSprite = TriangleSpriteFactory.Create("DarkSpiderMapTracker.Triangle", 64);
        }

        internal void OnMapAwake(Map map)
        {
            if (map == null)
                return;

            _lastMap = map;
            Clear();
            _nextPlayerSafetyScan = 0f;
        }

        internal void Tick(Map map)
        {
            if (map == null || Map.Instance == null)
                return;

            if (_lastMap != map)
                OnMapAwake(map);

            SafetyScanPlayers();
            UpdateTrackers(_enemyTrackers, isPlayerTracker: false);
            UpdateTrackers(_playerTrackers, isPlayerTracker: true);
        }

        internal void Register(object source)
        {
            if (source == null || Map.Instance == null || Map.Instance.CustomObject == null || Map.Instance.OverLayerParent == null)
                return;

            bool isEnemy = source is EnemyParent;
            bool isPlayer = source is PlayerAvatar;

            if (!isEnemy && !isPlayer)
                return;

            if (isPlayer)
            {
                PlayerAvatar playerAvatar = source as PlayerAvatar;
                if (IsLocalPlayer(playerAvatar))
                    return;
            }

            List<TrackedMapEntity> targetList = isPlayer ? _playerTrackers : _enemyTrackers;
            if (targetList.Any(entry => ReferenceEquals(entry.Source, source)))
                return;

            GameObject visualObject = GetVisualObject(source);
            if (visualObject == null)
                return;

            GameObject mapObject = Object.Instantiate(Map.Instance.CustomObject, Map.Instance.OverLayerParent);
            mapObject.name = "DarkSpider Map Tracker - " + visualObject.name;

            MapCustomEntity mapEntity = mapObject.GetComponent<MapCustomEntity>();
            if (mapEntity == null)
            {
                Object.Destroy(mapObject);
                return;
            }

            SpriteRenderer arrowRenderer = mapEntity.spriteRenderer != null
                ? mapEntity.spriteRenderer
                : mapObject.GetComponentInChildren<SpriteRenderer>(true);

            if (arrowRenderer == null)
            {
                Object.Destroy(mapObject);
                return;
            }

            mapEntity.Parent = visualObject.transform;

            arrowRenderer.sprite = _arrowSprite;
            arrowRenderer.enabled = false;
            arrowRenderer.sortingOrder += 10;

            SpriteRenderer outlineRenderer = CreateOutlineRenderer(mapEntity.transform, arrowRenderer);

            targetList.Add(new TrackedMapEntity
            {
                Source = source,
                MapEntity = mapEntity,
                ArrowRenderer = arrowRenderer,
                OutlineRenderer = outlineRenderer,
                Visible = false,
                IsPlayer = isPlayer
            });
        }

        internal void Show(object source)
        {
            TrackedMapEntity entry = Find(source);
            if (entry == null)
                return;

            if (entry.IsPlayer && source is PlayerAvatar playerAvatar && IsPlayerDead(playerAvatar))
                return;

            entry.Visible = true;
        }

        internal void Hide(object source)
        {
            TrackedMapEntity entry = Find(source);
            if (entry != null)
                entry.Visible = false;
        }

        internal void SwapCategory(object source, object otherSource)
        {
            TrackedMapEntity entry = Find(source);
            TrackedMapEntity otherEntry = Find(otherSource);
            if (entry == null || otherEntry == null)
                return;

            bool entryIsEnemy = _enemyTrackers.Contains(entry);
            bool otherEntryIsEnemy = _enemyTrackers.Contains(otherEntry);

            if (entryIsEnemy == otherEntryIsEnemy)
                return;

            if (entryIsEnemy)
            {
                _enemyTrackers.Remove(entry);
                _enemyTrackers.Add(otherEntry);
                _playerTrackers.Remove(otherEntry);
                _playerTrackers.Add(entry);
                entry.IsPlayer = true;
                otherEntry.IsPlayer = false;
            }
            else
            {
                _playerTrackers.Remove(entry);
                _playerTrackers.Add(otherEntry);
                _enemyTrackers.Remove(otherEntry);
                _enemyTrackers.Add(entry);
                entry.IsPlayer = false;
                otherEntry.IsPlayer = true;
            }
        }

        internal void SetAllVisible(bool visible)
        {
            SetTrackersVisible(_enemyTrackers, visible);
            SetTrackersVisible(_playerTrackers, visible);
        }

        internal void Clear()
        {
            DestroyTrackers(_enemyTrackers);
            DestroyTrackers(_playerTrackers);
            _enemyTrackers.Clear();
            _playerTrackers.Clear();
        }

        private void SafetyScanPlayers()
        {
            if (!_config.ShowPlayers.Value || Map.Instance == null || Time.time < _nextPlayerSafetyScan)
                return;

            _nextPlayerSafetyScan = Time.time + PlayerSafetyScanInterval;

            foreach (PlayerAvatar playerAvatar in Object.FindObjectsOfType<PlayerAvatar>())
            {
                if (playerAvatar == null || IsLocalPlayer(playerAvatar))
                    continue;

                Register(playerAvatar);

                if (!IsPlayerDead(playerAvatar))
                    Show(playerAvatar);
            }
        }

        private void UpdateTrackers(List<TrackedMapEntity> trackers, bool isPlayerTracker)
        {
            bool enabledByConfig = isPlayerTracker ? _config.ShowPlayers.Value : _config.ShowEnemies.Value;

            for (int i = trackers.Count - 1; i >= 0; i--)
            {
                TrackedMapEntity entry = trackers[i];
                if (!IsValid(entry))
                {
                    DestroyTracker(entry);
                    trackers.RemoveAt(i);
                    continue;
                }

                Transform parent = entry.MapEntity.Parent;
                if (parent == null)
                {
                    SetRendererPairEnabled(entry, false);
                    continue;
                }

                if (Map.Instance.Active)
                    Map.Instance.CustomPositionSet(entry.MapEntity.transform, parent);

                bool alive = !isPlayerTracker || !(entry.Source is PlayerAvatar playerAvatar) || !IsPlayerDead(playerAvatar);
                bool shouldRender = enabledByConfig && entry.Visible && alive;

                Color arrowColor = isPlayerTracker
                    ? ColorTools.ResolvePlayerColor(_config.PlayerColor.Value, _config.PlayerCustomHex.Value, entry.Source as PlayerAvatar)
                    : _config.GetEnemyColor();

                float scale = Mathf.Clamp(isPlayerTracker ? _config.PlayerArrowScale.Value : _config.EnemyArrowScale.Value, 0.25f, 3f);

                ApplyRenderer(entry.ArrowRenderer, arrowColor, scale, shouldRender);
                ApplyOutline(entry.OutlineRenderer, arrowColor.a, scale, shouldRender);
            }
        }

        private void ApplyRenderer(SpriteRenderer renderer, Color color, float scale, bool visible)
        {
            renderer.sprite = _arrowSprite;
            renderer.color = color;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.enabled = visible;
        }

        private void ApplyOutline(SpriteRenderer outlineRenderer, float alpha, float mainScale, bool mainVisible)
        {
            if (outlineRenderer == null)
                return;

            Color outlineColor = _config.GetOutlineColor();
            outlineColor.a = alpha;

            outlineRenderer.sprite = _arrowSprite;
            outlineRenderer.color = outlineColor;
            outlineRenderer.transform.localScale = Vector3.one * mainScale * Mathf.Clamp(_config.OutlineScale.Value, 1f, 2f);
            outlineRenderer.enabled = _config.OutlineEnabled.Value && mainVisible;
        }

        private SpriteRenderer CreateOutlineRenderer(Transform mapEntityTransform, SpriteRenderer arrowRenderer)
        {
            GameObject outlineObject = new GameObject("DarkSpider Map Tracker Outline");
            outlineObject.transform.SetParent(mapEntityTransform, false);
            outlineObject.transform.localPosition = arrowRenderer.transform.localPosition;
            outlineObject.transform.localRotation = arrowRenderer.transform.localRotation;
            outlineObject.transform.localScale = Vector3.one;

            SpriteRenderer outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = _arrowSprite;
            outlineRenderer.color = _config.GetOutlineColor();
            outlineRenderer.sortingLayerID = arrowRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = arrowRenderer.sortingOrder - 1;
            outlineRenderer.enabled = false;

            return outlineRenderer;
        }

        private static GameObject GetVisualObject(object source)
        {
            if (source is PlayerAvatar playerAvatar)
                return playerAvatar.playerAvatarVisuals != null ? playerAvatar.playerAvatarVisuals.gameObject : playerAvatar.gameObject;

            if (!(source is EnemyParent enemyParent))
                return null;

            Enemy enemy = ReflectionCache.GetEnemy(enemyParent);
            if (enemy == null)
                return null;

            GameObject visualObject = null;

            if (enemyParent.enemyName != "Bella")
            {
                visualObject = TryGetVisionObject(enemy);
                if (visualObject == null)
                    visualObject = TryGetAnimatorObject(enemyParent.EnableObject);
            }
            else
            {
                visualObject = TryGetTricycleTargetObject(enemy);
            }

            return visualObject != null ? visualObject : enemy.gameObject;
        }

        private static GameObject TryGetVisionObject(Enemy enemy)
        {
            try
            {
                EnemyVision vision = enemy.GetComponentInChildren<EnemyVision>();
                return vision?.VisionTransform != null ? vision.VisionTransform.gameObject : null;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read enemy vision transform: {exception.Message}");
                return null;
            }
        }

        private static GameObject TryGetTricycleTargetObject(Enemy enemy)
        {
            try
            {
                EnemyTricycle tricycle = enemy.GetComponentInChildren<EnemyTricycle>();
                return tricycle?.followTargetTransform != null ? tricycle.followTargetTransform.gameObject : null;
            }
            catch (Exception exception)
            {
                Plugin.Log?.LogDebug($"Failed to read EnemyTricycle target: {exception.Message}");
                return null;
            }
        }

        private static GameObject TryGetAnimatorObject(GameObject rootObject)
        {
            if (rootObject == null)
                return null;

            Component[] components = rootObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                Type type = component.GetType();
                if (type.FullName == "UnityEngine.Animator" || type.Name == "Animator")
                    return component.gameObject;
            }

            return null;
        }

        private static bool IsLocalPlayer(PlayerAvatar playerAvatar)
        {
            return playerAvatar != null && playerAvatar == PlayerController.instance?.playerAvatarScript;
        }

        private static bool IsPlayerDead(PlayerAvatar playerAvatar)
        {
            return playerAvatar == null || ReflectionCache.IsPlayerDisabled(playerAvatar);
        }

        private TrackedMapEntity Find(object source)
        {
            if (source == null)
                return null;

            return _enemyTrackers.FirstOrDefault(entry => ReferenceEquals(entry.Source, source)) ??
                   _playerTrackers.FirstOrDefault(entry => ReferenceEquals(entry.Source, source));
        }

        private static bool IsValid(TrackedMapEntity entry)
        {
            return entry != null &&
                   entry.Source != null &&
                   entry.MapEntity != null &&
                   entry.ArrowRenderer != null;
        }

        private static void SetTrackersVisible(List<TrackedMapEntity> trackers, bool visible)
        {
            foreach (TrackedMapEntity entry in trackers)
                SetRendererPairEnabled(entry, visible);
        }

        private static void SetRendererPairEnabled(TrackedMapEntity entry, bool visible)
        {
            if (entry?.ArrowRenderer != null)
                entry.ArrowRenderer.enabled = visible;

            if (entry?.OutlineRenderer != null)
                entry.OutlineRenderer.enabled = visible;
        }

        private static void DestroyTrackers(List<TrackedMapEntity> trackers)
        {
            foreach (TrackedMapEntity entry in trackers)
                DestroyTracker(entry);
        }

        private static void DestroyTracker(TrackedMapEntity entry)
        {
            if (entry?.MapEntity != null)
                Object.Destroy(entry.MapEntity.gameObject);
        }
    }
}
