using KWEngine3.Model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;

namespace KWEngine3.Renderer
{
    internal static class RendererArmature
    {
        // Bildgröße der erzeugten Armature-Textur
        private const int TEX_WIDTH  = 512;
        private const int TEX_HEIGHT = 512;

        /// <summary>
        /// Zeichnet die Armature des Modells als 2D-Diagramm in eine GL-Textur
        /// und speichert die Textur-ID in model.ArmatureTextureID.
        /// </summary>
        public static void Draw(GeoModel model)
        {
            if (model == null || model.Armature == null || !model.HasBones)
                return;

            // 1. Alle Knochen-Knoten mit akkumulierten Weltpositionen sammeln
            var boneSet = new HashSet<string>(model.BoneNames);
            // key   = GeoNode des Knochens
            // value = (Weltposition, Elternknoten oder null)
            var nodes = new Dictionary<GeoNode, (Vector3 pos, GeoNode parent)>();
            CollectBoneNodes(model.Armature, Matrix4.Identity, null, boneSet, nodes);

            if (nodes.Count == 0)
                return;

            // 2. AABB der Knochen-Positionen (Front-View: X/Y)
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var (pos, _) in nodes.Values)
            {
                if (pos.X < minX) minX = pos.X;
                if (pos.X > maxX) maxX = pos.X;
                if (pos.Y < minY) minY = pos.Y;
                if (pos.Y > maxY) maxY = pos.Y;
            }

            float rangeX = maxX - minX;
            float rangeY = maxY - minY;

            // Uniforme Skalierung: Aspektverhältnis der Knochen-AABB beibehalten,
            // 85 % der Textur nutzen und den Inhalt mittig platzieren.
            const float FILL  = 0.85f;
            const float MARGIN = 4f; // px Mindestabstand zum Bildrand für Labels
            float scale    = Math.Min(TEX_WIDTH  / Math.Max(rangeX, 1e-5f),
                                      TEX_HEIGHT / Math.Max(rangeY, 1e-5f)) * FILL;
            float contentW = rangeX * scale;
            float contentH = rangeY * scale;
            float offX     = (TEX_WIDTH  - contentW) * 0.5f;
            float offY     = (TEX_HEIGHT - contentH) * 0.5f;

            // Projektion: Weltkoordinate → Canvas-Pixel
            // Y wird gespiegelt (Welt-Y nach oben = Canvas-Y nach oben)
            SKPoint Project(Vector3 p) => new(
                offX + (p.X - minX) * scale,
                (TEX_HEIGHT - offY) - (p.Y - minY) * scale
            );

            // 3. SkiaSharp-Bitmap aufbauen
            using var bitmap = new SKBitmap(TEX_WIDTH, TEX_HEIGHT, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(new SKColor(28, 28, 38));

            using var linePaint = new SKPaint
            {
                Color       = new SKColor(100, 100, 130),
                StrokeWidth = 1.2f,
                IsAntialias = true,
                Style       = SKPaintStyle.Stroke
            };
            using var dotPaint = new SKPaint
            {
                IsAntialias = true,
                Style       = SKPaintStyle.Fill
            };
            using var labelFont = new SKFont(SKTypeface.Default, 11f);
            using var labelPaint = new SKPaint
            {
                Color       = new SKColor(230, 230, 230),
                IsAntialias = true
            };
            using var labelShadowFont = new SKFont(SKTypeface.Default, 11f);
            using var labelShadowPaint = new SKPaint
            {
                Color       = new SKColor(0, 0, 0, 180),
                IsAntialias = true
            };
            using var leaderLinePaint = new SKPaint
            {
                Color       = new SKColor(140, 140, 160, 180),
                StrokeWidth = 1f,
                IsAntialias = false,
                Style       = SKPaintStyle.Stroke,
                PathEffect  = SKPathEffect.CreateDash(new float[] { 4f, 3f }, 0f)
            };

            // Projizierte Pixel-Positionen vorberechnen
            var screenPos = new Dictionary<GeoNode, SKPoint>(nodes.Count);
            foreach (var (node, (pos, _)) in nodes)
                screenPos[node] = Project(pos);

            // 3a. Verbindungslinien parent → child
            foreach (var (node, (_, parentNode)) in nodes)
            {
                if (parentNode != null && nodes.ContainsKey(parentNode))
                    canvas.DrawLine(screenPos[parentNode], screenPos[node], linePaint);
            }

            // 3b. Punkte + Labels
            // placedRects enthält sowohl Dot-Bounding-Boxes als auch platzierte Label-Rects,
            // damit Labels weder andere Labels noch Knochen-Dots überdecken.
            var placedRects = new List<SKRect>();
            const float DOT_R = 3.5f;

            int boneIdx = 0;
            foreach (var (node, _) in nodes)
            {
                SKPoint pt = screenPos[node];

                // Farbe nach Bone-Index (goldener Winkel für maximale Farbstreuung)
                float hue = (boneIdx * 137.5f) % 360f;
                dotPaint.Color = SKColor.FromHsl(hue, 75f, 62f);
                canvas.DrawCircle(pt, DOT_R, dotPaint);

                // Dot-Bereich in placedRects registrieren (etwas größer als der Kreis)
                placedRects.Add(SKRect.Create(pt.X - DOT_R - 1f, pt.Y - DOT_R - 1f,
                                              (DOT_R + 1f) * 2f, (DOT_R + 1f) * 2f));

                // Heuristik: thumb- und toe-Knochen (außer toebase) bekommen kein Label
                string label = (node.NameWithoutFBXSuffix != null && node.NameWithoutFBXSuffix.Length > 0)
                    ? node.NameWithoutFBXSuffix
                    : node.Name;
                if (ShouldSkipLabel(label))
                {
                    boneIdx++;
                    continue;
                }

                float textW = labelFont.MeasureText(label);
                float textH = 12f;   // Schriftgröße 11f
                float lineH = textH + 3f; // Rechteckhöhe inkl. kleiner Puffer

                // Kandidaten: zentriert über/unter dem Dot, gestapelt in aufsteigendem Abstand.
                // Kein right/left mehr – Labels sollen nie auf Dot-Höhe liegen.
                // isClose = true  → direkt über/unter (step 1), keine Leader-Linie nötig
                // isClose = false → weiter weg gestapelt, gestrichelte Leader-Linie
                const int STACK_STEPS = 10;
                var candidates = new List<(float tx, float ty, SKRect rect, bool isClose)>(STACK_STEPS * 2);

                float baseGap = DOT_R + 2f;
                for (int step = 1; step <= STACK_STEPS; step++)
                {
                    bool isClose = step == 1;
                    float aboveY  = pt.Y - baseGap - step * lineH;
                    float belowYb = pt.Y + baseGap + (step - 1) * lineH;
                    float belowYt = belowYb + textH;

                    // direkt über dem Dot zentriert
                    candidates.Add((
                        pt.X - textW * 0.5f,
                        aboveY + textH,
                        SKRect.Create(pt.X - textW * 0.5f, aboveY, textW, lineH),
                        isClose));
                    // direkt unter dem Dot zentriert
                    candidates.Add((
                        pt.X - textW * 0.5f,
                        belowYt,
                        SKRect.Create(pt.X - textW * 0.5f, belowYb, textW, lineH),
                        isClose));
                }

                foreach (var (tx, ty, rect, isClose) in candidates)
                {
                    // Bildrand-Check
                    if (rect.Left   < MARGIN              ||
                        rect.Right  > TEX_WIDTH  - MARGIN ||
                        rect.Top    < MARGIN              ||
                        rect.Bottom > TEX_HEIGHT - MARGIN)
                        continue;

                    // Overlap-Check
                    bool overlaps = false;
                    foreach (var placed in placedRects)
                    {
                        if (RectsOverlap(rect, placed)) { overlaps = true; break; }
                    }
                    if (overlaps) continue;

                    // Gestrichelte Leader-Linie wenn das Label weiter weg platziert wurde
                    if (!isClose)
                    {
                        // Ankerpunkt auf dem Label: die zum Dot nächste Kante (Mitte)
                        float lineEndX = rect.MidX;
                        float lineEndY = rect.MidY < pt.Y ? rect.Bottom : rect.Top;
                        canvas.DrawLine(pt.X, pt.Y, lineEndX, lineEndY, leaderLinePaint);
                    }

                    // Text mit Schatten
                    canvas.DrawText(label, tx + 1f, ty + 1f, labelShadowFont, labelShadowPaint);
                    canvas.DrawText(label, tx,       ty,      labelFont, labelPaint);
                    placedRects.Add(rect);
                    break;
                }

                boneIdx++;
            }

            // 4. Bitmap als GL-Textur hochladen (bereits Rgba8888, direkt verwendbar)
            byte[] bytes = bitmap.Bytes;

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba8,
                TEX_WIDTH, TEX_HEIGHT, 0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                bytes);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,     (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,     (float)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //testweise als png speichern:
            /*
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite("c:/temp/armature.png"))
            {
                data.SaveTo(stream);
            }
            */

            model.ArmatureTextureID = texId;
        }

        // ------------------------------------------------------------------ //

        /// <summary>
        /// Traversiert die GeoNode-Hierarchie rekursiv und sammelt alle Knoten,
        /// die echte Knochen sind (d.h. in boneSet enthalten), mit ihrer
        /// akkumulierten Weltposition und ihrem nächsten Eltern-Knochen.
        /// </summary>
        private static void CollectBoneNodes(
            GeoNode node,
            Matrix4 parentWorld,
            GeoNode parentBone,
            HashSet<string> boneSet,
            Dictionary<GeoNode, (Vector3, GeoNode)> result)
        {
            // Weltmatrix dieses Knotens (OpenTK: Zeilen-Vektor-Konvention)
            Matrix4 world = node.Transform * parentWorld;

            bool isBone = boneSet.Contains(node.Name)
                       || (node.NameWithoutFBXSuffix != null
                           && boneSet.Contains(node.NameWithoutFBXSuffix));

            GeoNode nextParent = parentBone;
            if (isBone)
            {
                Vector3 pos = world.Row3.Xyz; // Translationsanteil
                result[node] = (pos, parentBone);
                nextParent = node;
            }

            foreach (GeoNode child in node.Children)
                CollectBoneNodes(child, world, nextParent, boneSet, result);
        }

        /// <summary>
        /// Gibt true zurück für Knochen, die kein Label benötigen:
        /// thumb-Knochen (alle) und toe-Knochen (außer toebase).
        /// </summary>
        private static bool ShouldSkipLabel(string name)
        {
            string lower = name.ToLowerInvariant();
            if (lower.Contains("toebase")) return false; // Ausnahme: toebase immer labeln
            return lower.Contains("thumb") || lower.Contains("toe");
        }

        /// <summary>Einfacher AABB-Überschneidungstest für zwei SKRect.</summary>
        private static bool RectsOverlap(SKRect a, SKRect b)
        {
            return !(a.Right  < b.Left  ||
                     b.Right  < a.Left  ||
                     a.Bottom < b.Top   ||
                     b.Bottom < a.Top);
        }
    }
}
