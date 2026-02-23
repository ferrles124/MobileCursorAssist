using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MobileCursorAssist
{
    /// <summary>
    /// Ekranda imlecin üzerine çizilen dairesel gösterge.
    /// Texture CPU tarafında piksel piksel oluşturulur,
    /// bu sayede harici sprite dosyasına gerek kalmaz.
    /// </summary>
    public class CursorOverlay
    {
        // ── Alanlar ───────────────────────────────────────────────────────
        private Texture2D? _circleTexture;
        private readonly int _radius;
        private readonly Color _color;

        // Dış kenarlık kalınlığı (piksel)
        private const int BorderThickness = 3;

        // ── Özellikler ────────────────────────────────────────────────────

        /// <summary>Dairenin ekrandaki merkez noktası.</summary>
        public Vector2 Position { get; set; } = new Vector2(400, 300);

        /// <summary>False yapılırsa daire çizilmez.</summary>
        public bool Visible { get; set; } = true;

        // ── Constructor ───────────────────────────────────────────────────

        public CursorOverlay(int radius, Color color)
        {
            _radius = radius;
            _color = color;
        }

        // ── Public Metodlar ───────────────────────────────────────────────

        /// <summary>
        /// Daire texture'ını GPU'ya yükler.
        /// GameLaunched eventinden sonra çağrılmalı.
        /// </summary>
        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            int size = _radius * 2;
            _circleTexture = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(_radius - 0.5f, _radius - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);

                    // Sadece kenarlık bölgesini boya, iç kısım şeffaf
                    if (dist <= _radius && dist >= _radius - BorderThickness)
                    {
                        // Kenar yumuşatma: kenara yaklaştıkça solar
                        float alpha = 1f - Math.Abs(dist - (_radius - BorderThickness / 2f))
                                          / (BorderThickness / 2f);
                        alpha = Math.Clamp(alpha, 0f, 1f);

                        data[y * size + x] = new Color(
                            _color.R,
                            _color.G,
                            _color.B,
                            (byte)(_color.A * alpha)
                        );
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }

            _circleTexture.SetData(data);
        }

        /// <summary>
        /// SpriteBatch açıkken çağrılmalı.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible || _circleTexture == null) return;

            spriteBatch.Draw(
                _circleTexture,
                new Vector2(Position.X - _radius, Position.Y - _radius),
                null,
                Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 1f,
                SpriteEffects.None,
                layerDepth: 1f
            );
        }

        /// <summary>Texture belleğini serbest bırakır.</summary>
        public void Dispose()
        {
            _circleTexture?.Dispose();
        }
    }
}