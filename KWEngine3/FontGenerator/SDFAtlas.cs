using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KWEngine3.FontGenerator
{
    internal sealed class AtlasRoot
    {
        [JsonPropertyName("atlas")]
        public AtlasInfo Atlas { get; set; } = default!;

        [JsonPropertyName("metrics")]
        public FontMetrics Metrics { get; set; } = default!;

        [JsonPropertyName("glyphs")]
        public List<Glyph> Glyphs { get; set; } = new();

        // Kann leer sein
        [JsonPropertyName("kerning")]
        public List<KerningPair> Kerning { get; set; } = new();
    }

    internal sealed class AtlasInfo
    {
        /// <summary>
        /// z. B. "msdf", "mtsdf", "sdf" ...
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("distanceRange")]
        public double DistanceRange { get; set; }

        // Optional – nicht in allen Dateien vorhanden
        [JsonPropertyName("distanceRangeMiddle")]
        public double? DistanceRangeMiddle { get; set; }

        /// <summary>
        /// Font-Größe in px per em (aus msdf-atlas-gen JSON "size").
        /// </summary>
        [JsonPropertyName("size")]
        public double Size { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// "bottom" oder "top"
        /// </summary>
        [JsonPropertyName("yOrigin")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public YOrigin YOrigin { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum YOrigin
    {
        Bottom,
        Top
    }

    internal sealed class FontMetrics
    {
        [JsonPropertyName("emSize")]
        public double EmSize { get; set; }

        [JsonPropertyName("lineHeight")]
        public double LineHeight { get; set; }

        [JsonPropertyName("ascender")]
        public double Ascender { get; set; }

        [JsonPropertyName("descender")]
        public double Descender { get; set; }

        [JsonPropertyName("underlineY")]
        public double UnderlineY { get; set; }

        [JsonPropertyName("underlineThickness")]
        public double UnderlineThickness { get; set; }
    }

    internal sealed class Glyph
    {
        /// <summary>
        /// Unicode-Codepoint (dezimal).
        /// </summary>
        [JsonPropertyName("unicode")]
        public int Unicode { get; set; }

        /// <summary>
        /// Horizontal Advance in em.
        /// </summary>
        [JsonPropertyName("advance")]
        public double Advance { get; set; }

        /// <summary>
        /// Glyph-Quad in em relativ zur Baseline/Cursorposition.
        /// Kann fehlen (z. B. für Space).
        /// </summary>
        [JsonPropertyName("planeBounds")]
        public PlaneBounds PlaneBounds { get; set; }

        /// <summary>
        /// Glyph-Rechteck im Atlas (Pixel).
        /// Kann fehlen (z. B. für Space).
        /// </summary>
        [JsonPropertyName("atlasBounds")]
        public AtlasBounds AtlasBounds { get; set; }
    }

    internal sealed class PlaneBounds
    {
        [JsonPropertyName("left")]
        public double Left { get; set; }

        [JsonPropertyName("bottom")]
        public double Bottom { get; set; }

        [JsonPropertyName("right")]
        public double Right { get; set; }

        [JsonPropertyName("top")]
        public double Top { get; set; }
    }

    internal sealed class AtlasBounds
    {
        [JsonPropertyName("left")]
        public double Left { get; set; }

        [JsonPropertyName("bottom")]
        public double Bottom { get; set; }

        [JsonPropertyName("right")]
        public double Right { get; set; }

        [JsonPropertyName("top")]
        public double Top { get; set; }
    }

    /// <summary>
    /// Typische Darstellung von Kerning-Paaren in msdf-atlas-gen:
    /// (Beispielstruktur – falls deine Datei andere Feldnamen nutzt, anpassen.)
    /// </summary>
    internal sealed class KerningPair
    {
        [JsonPropertyName("unicode1")]
        public int Unicode1 { get; set; }

        [JsonPropertyName("unicode2")]
        public int Unicode2 { get; set; }

        /// <summary>
        /// Kerning-Adjust in em (additiv zum Advance des ersten Zeichens).
        /// </summary>
        [JsonPropertyName("advance")]
        public double Advance { get; set; }
    }
}