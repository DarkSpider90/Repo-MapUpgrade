using UnityEngine;

namespace DarkSpider.MapTracker
{
    internal static class TriangleSpriteFactory
    {
        internal static Sprite Create(string name, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.name = name;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color clear = new Color(1f, 1f, 1f, 0f);
            Color white = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                    texture.SetPixel(x, y, clear);
            }

            Vector2 tip = new Vector2(size * 0.5f, size * 0.88f);
            Vector2 left = new Vector2(size * 0.18f, size * 0.18f);
            Vector2 right = new Vector2(size * 0.82f, size * 0.18f);
            Vector2[] polygon = { tip, right, left };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsInsidePolygon(point, polygon))
                        texture.SetPixel(x, y, white);
                }
            }

            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static bool IsInsidePolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int previous = polygon.Length - 1;

            for (int current = 0; current < polygon.Length; current++)
            {
                bool crossesY = (polygon[current].y > point.y) != (polygon[previous].y > point.y);
                if (crossesY)
                {
                    float intersectionX = (polygon[previous].x - polygon[current].x) *
                                          (point.y - polygon[current].y) /
                                          (polygon[previous].y - polygon[current].y) +
                                          polygon[current].x;

                    if (point.x < intersectionX)
                        inside = !inside;
                }

                previous = current;
            }

            return inside;
        }
    }
}
