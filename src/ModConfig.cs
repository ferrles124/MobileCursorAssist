namespace MobileCursorAssist
{
    /// <summary>
    /// config.json ile eşleşen ayar sınıfı.
    /// SMAPI bu sınıfı serialize/deserialize eder.
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Tek tıkta tetiklenecek tuş.
        /// SButton enum değeri: MouseLeft, MouseRight, A, B, vb.
        /// </summary>
        public string SingleTapAction { get; set; } = "MouseLeft";

        /// <summary>
        /// Çift tıkta tetiklenecek tuş.
        /// </summary>
        public string DoubleTapAction { get; set; } = "MouseLeft";

        /// <summary>
        /// Uzun basmada tetiklenecek tuş.
        /// </summary>
        public string LongPressAction { get; set; } = "MouseRight";

        /// <summary>
        /// Çift tık olarak sayılacak iki tık arası maksimum süre (milisaniye).
        /// 300ms genellikle ideal değerdir.
        /// </summary>
        public int DoubleTapMaxMs { get; set; } = 300;

        /// <summary>
        /// Uzun basma olarak sayılacak minimum basılı kalma süresi (milisaniye).
        /// </summary>
        public int LongPressMinMs { get; set; } = 600;

        /// <summary>
        /// Ekrandaki dairenin yarıçapı (piksel).
        /// </summary>
        public int CircleRadius { get; set; } = 40;

        // Renk bileşenleri ayrı tutuldu çünkü
        // SMAPI config Color tipini doğrudan desteklemez.
        public int CircleColorR { get; set; } = 255;
        public int CircleColorG { get; set; } = 255;
        public int CircleColorB { get; set; } = 255;

        /// <summary>
        /// 0 = tamamen saydam, 255 = tamamen opak.
        /// </summary>
        public int CircleColorA { get; set; } = 180;
    }
}