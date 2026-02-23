using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MobileCursorAssist
{
    /// <summary>
    /// Modun ana giriş noktası.
    /// SMAPI bu sınıfı yükler ve Entry() metodunu çağırır.
    /// </summary>
    public class ModEntry : Mod
    {
        // ── Bağımlılıklar ─────────────────────────────────────────────────
        private ModConfig _config = null!;
        private CursorOverlay _overlay = null!;

        // ── Gesture durumu ────────────────────────────────────────────────

        /// <summary>Şu an sol tuş basılı mı?</summary>
        private bool _isPressed = false;

        /// <summary>Son basma başlangıç zamanı.</summary>
        private DateTime _pressStart;

        /// <summary>Son bırakma zamanı (çift tık tespiti için).</summary>
        private DateTime _lastReleaseTime = DateTime.MinValue;

        /// <summary>Bu basışta uzun basma zaten tetiklendi mi?</summary>
        private bool _longPressFired = false;

        /// <summary>Güncel imleç pozisyonu.</summary>
        private Vector2 _cursorPos = new Vector2(400, 300);

        // ── SMAPI Entry ───────────────────────────────────────────────────

        public override void Entry(IModHelper helper)
        {
            // Config'i oku (yoksa varsayılanlarla oluşturur)
            _config = helper.ReadConfig<ModConfig>();

            // Overlay'i oluştur (texture henüz yok, sadece ayarlar)
            _overlay = new CursorOverlay(
                _config.CircleRadius,
                new Color(
                    _config.CircleColorR,
                    _config.CircleColorG,
                    _config.CircleColorB,
                    _config.CircleColorA
                )
            );

            // Event'lere abone ol
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderedHud   += OnRenderedHud;
        }

        // ── Event Handlers ────────────────────────────────────────────────

        /// <summary>
        /// Oyun tamamen yüklendikten sonra texture oluştur.
        /// GraphicsDevice bu noktada hazır olur.
        /// </summary>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            _overlay.LoadContent(Game1.graphics.GraphicsDevice);
            Monitor.Log("MobileCursorAssist yüklendi.", LogLevel.Info);
        }

        /// <summary>
        /// Her kare çağrılır (~60fps).
        /// Mouse durumunu okur, gesture tipini belirler, aksiyonu tetikler.
        /// </summary>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // Sadece oyun dünyası hazırken çalış
            if (!Context.IsWorldReady) return;

            // 1) İmleç pozisyonunu al
            var mouseState = Mouse.GetState();
            _cursorPos = new Vector2(mouseState.X, mouseState.Y);
            _overlay.Position = _cursorPos;

            // 2) Sol tuş durumu
            bool currentlyDown = mouseState.LeftButton == ButtonState.Pressed;

            if (currentlyDown && !_isPressed)
            {
                // ── Basma başladı ──────────────────────────────────────
                _isPressed = true;
                _pressStart = DateTime.Now;
                _longPressFired = false;
            }
            else if (!currentlyDown && _isPressed)
            {
                // ── Bırakıldı ──────────────────────────────────────────
                _isPressed = false;

                if (!_longPressFired)
                {
                    // Çift tık mı kontrol et
                    double msSinceLastRelease = (DateTime.Now - _lastReleaseTime).TotalMilliseconds;

                    if (msSinceLastRelease <= _config.DoubleTapMaxMs)
                    {
                        // ✓ Çift tık
                        _lastReleaseTime = DateTime.MinValue; // sıfırla
                        Monitor.Log("Gesture: Çift Tık", LogLevel.Debug);
                        FireAction(_config.DoubleTapAction);
                    }
                    else
                    {
                        // ✓ Tek tık (şimdilik) — bir sonraki tık çift tık yapabilir
                        _lastReleaseTime = DateTime.Now;
                        Monitor.Log("Gesture: Tek Tık", LogLevel.Debug);
                        FireAction(_config.SingleTapAction);
                    }
                }
            }
            else if (currentlyDown && _isPressed && !_longPressFired)
            {
                // ── Basılı tutma kontrolü ──────────────────────────────
                double heldMs = (DateTime.Now - _pressStart).TotalMilliseconds;

                if (heldMs >= _config.LongPressMinMs)
                {
                    _longPressFired = true;
                    Monitor.Log("Gesture: Uzun Basma", LogLevel.Debug);
                    FireAction(_config.LongPressAction);
                }
            }
        }

        /// <summary>
        /// HUD render edilirken daire çiz.
        /// RenderedHud, UI'nin üstünde ama menülerin altında çizer.
        /// </summary>
        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // SMAPI bu event'te SpriteBatch'i zaten açmış durumda
            // Begin/End çağrısına gerek yok
            _overlay.Draw(e.SpriteBatch);
        }

        // ── Yardımcı Metodlar ─────────────────────────────────────────────

        /// <summary>
        /// config.json'dan gelen tuş adını parse edip oyun aksiyonunu tetikler.
        /// </summary>
        private void FireAction(string actionKey)
        {
            if (!Enum.TryParse<SButton>(actionKey, ignoreCase: true, out SButton button))
            {
                Monitor.Log($"Geçersiz aksiyon tuşu: '{actionKey}'. " +
                            "SButton değerlerini kontrol edin.", LogLevel.Warn);
                return;
            }

            // İmleci doğru pozisyona taşı
            Mouse.SetPosition((int)_cursorPos.X, (int)_cursorPos.Y);

            // Tuşa göre oyun metodunu çağır
            try
            {
                switch (button)
                {
                    case SButton.MouseLeft:
                        // Etkileşim / konuşma / toplama
                        Game1.pressActionButton(
                            Game1.input.GetKeyboardState(),
                            Game1.input.GetMouseState(),
                            Game1.input.GetGamePadState()
                        );
                        break;

                    case SButton.MouseRight:
                        // Alet kullanma
                        Game1.pressUseToolButton();
                        break;

                    default:
                        // Klavye tuşları için SMAPI'nin input simülasyonunu kullan
                        // Not: SMAPI doğrudan tuş enjeksiyonu desteklemez,
                        // bu yüzden log ile bildir
                        Monitor.Log($"'{button}' tuşu simülasyonu şu an " +
                                    "sadece MouseLeft/MouseRight için destekleniyor.",
                                    LogLevel.Warn);
                        break;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"FireAction hatası ({actionKey}): {ex.Message}", LogLevel.Error);
            }
        }
    }
}