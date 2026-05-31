using UnityEngine;

namespace DarkSpider.MapTracker
{
    internal sealed class TrackedMapEntity
    {
        internal object Source;
        internal MapCustomEntity MapEntity;
        internal SpriteRenderer ArrowRenderer;
        internal SpriteRenderer OutlineRenderer;
        internal bool Visible;
        internal bool IsPlayer;
    }
}
